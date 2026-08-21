# Korp — Sistema de Emissão de Notas Fiscais

Aplicação desenvolvida para o teste técnico da Korp. O sistema permite cadastrar produtos, criar notas fiscais com múltiplos itens e imprimir uma nota aberta, fechando-a e baixando o estoque de forma transacional.

## Arquitetura

```text
Angular (porta 4200)
   ├── HTTP ──> Inventory Service (porta 5277) ──> inventory_db
   └── HTTP ──> Billing Service   (porta 5227) ──> billing_db
                         └── HTTP ──> Inventory Service
```

- **Inventory Service:** cadastro de produtos, consulta e baixa atômica de estoque.
- **Billing Service:** criação, numeração sequencial, consulta, impressão e fechamento de notas.
- **Bancos separados:** cada microsserviço é proprietário dos seus dados em MySQL.
- **Comunicação:** o Billing Service consulta e solicita a baixa ao Inventory Service por HTTP.

## Funcionalidades

- Cadastro de produto com código único, descrição e saldo não negativo.
- Criação de nota com um ou mais produtos e status inicial `Open` (Aberta na interface).
- Numeração sequencial protegida contra criações simultâneas.
- Botão de impressão disponível somente para notas abertas.
- Indicador `Processando...` durante a impressão.
- Baixa transacional de todos os itens, fechamento da nota e documento para impressão pelo navegador após o sucesso.
- Feedback para produto duplicado, saldo insuficiente e serviço indisponível.
- Dashboard com totais de produtos e notas abertas/fechadas.
- Layout responsivo.

## Consistência, concorrência e idempotência

A impressão usa a chave estável `billing-invoice-{id}`. O Inventory Service persiste essa chave em `StockOperations` na mesma transação da baixa. Se a resposta HTTP for perdida depois do `commit`, uma nova tentativa reconhece a operação e responde com sucesso sem descontar o saldo novamente.

A baixa utiliza um `UPDATE` condicional no banco:

```sql
UPDATE Products
SET StockQuantity = StockQuantity - @quantity
WHERE Id = @productId AND StockQuantity >= @quantity;
```

Assim, duas notas concorrendo pelo último item não conseguem deixar o estoque negativo: apenas uma alteração afeta uma linha; a outra recebe `409 Conflict`. Itens repetidos também são agrupados antes da validação.

A numeração usa uma linha em `InvoiceSequences`, lida com `SELECT ... FOR UPDATE` dentro de transação. Há ainda um índice único em `Invoices.Number` como garantia adicional.

## Cenário obrigatório de falha

Para demonstrar a recuperação:

1. Crie um produto e uma nota aberta.
2. Pare o Inventory Service.
3. Clique em **Imprimir**.
4. O Billing Service tenta novamente uma vez e retorna `503 Service Unavailable`; a interface informa que o estoque está indisponível e a nota permanece aberta.
5. Inicie novamente o Inventory Service e repita a impressão.
6. A nota é fechada e o saldo é baixado uma única vez.

O timeout do cliente HTTP é de cinco segundos. A repetição automática do `POST` é segura por causa da chave de idempotência.

## Detalhamento técnico

### Angular e ciclos de vida

Os componentes standalone `Dashboard`, `Products` e `Invoices` implementam `OnInit` e usam `ngOnInit()` para carregar os dados iniciais. Não foi necessário `OnDestroy`, pois os `Observable` do `HttpClient` completam após uma única resposta e não existem subscriptions de longa duração.

### RxJS

- Os serviços retornam `Observable<T>` do `HttpClient`.
- Os componentes tratam sucesso e falha com `subscribe({ next, error })`.
- `forkJoin` carrega produtos e notas do dashboard em paralelo.
- `finalize` encerra os indicadores de carregamento mesmo quando ocorre erro.

### Bibliotecas do frontend

| Biblioteca | Finalidade |
|---|---|
| Angular 21 | Componentes standalone e estrutura da SPA |
| Angular Router | Navegação entre dashboard, produtos e notas |
| Angular Forms | Formulários template-driven e validação |
| Angular HttpClient | Integração HTTP com os microsserviços |
| RxJS 7 | Composição assíncrona, `Observable`, `forkJoin` e `finalize` |
| Vitest | Testes automatizados do frontend |

Não foi usada biblioteca visual externa. A interface foi construída com templates Angular, HTML semântico e SCSS próprios.

### Backend C#

Os dois serviços usam C# com .NET 10, ASP.NET Core Web API e Entity Framework Core. O provider `MySql.EntityFrameworkCore` faz a integração com MySQL; `HttpClientFactory` configura a comunicação entre serviços.

Não há implementação em Golang, portanto gerenciamento de dependências com `go.mod` não se aplica. No C#, as dependências são declaradas nos arquivos `.csproj` e restauradas pelo NuGet com `dotnet restore`.

### LINQ

LINQ é usado junto ao EF Core e em memória para:

- `AnyAsync` e `FirstOrDefaultAsync`: existência e busca de registros;
- `Include`: carregamento dos itens de cada nota;
- `OrderBy` e `ToListAsync`: ordenação e materialização das consultas;
- `GroupBy`, `Select` e `Sum`: consolidação de itens repetidos;
- `Where` com `ExecuteUpdateAsync`: fechamento condicional da nota;
- `AsNoTracking`: leituras usadas apenas para validação.

### Erros e exceções

- A validação por Data Annotations e `[ApiController]` retorna `400 Bad Request`.
- Recursos inexistentes retornam `404 Not Found`.
- Código duplicado, nota já fechada e saldo insuficiente retornam `409 Conflict`.
- Falhas e timeouts do Inventory Service são convertidos em `503 Service Unavailable`.
- Um `IExceptionHandler` global registra exceções inesperadas e devolve `ProblemDetails` com `500`, sem expor stack trace ao cliente.
- Transações são revertidas automaticamente quando qualquer item da baixa falha.
- `CancellationToken` propaga cancelamento do cliente às consultas e chamadas HTTP.

## Banco de dados

O projeto usa MySQL com persistência física e migrations do EF Core. As bases esperadas são:

- `inventory_db`
- `billing_db`

Os arquivos reais `appsettings.json` não são versionados. Copie o exemplo de cada serviço:

```powershell
Copy-Item inventory-service/appsettings.example.json inventory-service/appsettings.json
Copy-Item billing-service/appsettings.example.json billing-service/appsettings.json
```

Edite usuário e senha nas connection strings. `SslMode=Disabled` é adequado para a instância local sem TLS; em produção, configure certificados e um modo SSL compatível. A URL do estoque usada pelo Billing Service fica em `Services:InventoryUrl`.

## Como executar

Pré-requisitos: .NET SDK 10, Node.js, npm e MySQL 8 ou superior.

```powershell
cd inventory-service
dotnet restore
dotnet ef database update
dotnet run
```

Em outro terminal:

```powershell
cd billing-service
dotnet restore
dotnet ef database update
dotnet run
```

Em um terceiro terminal:

```powershell
cd frontend
npm install
npm start
```

Acesse `http://localhost:4200`.

## Testes e validação

```powershell
dotnet build inventory-service/inventory-service.csproj
dotnet build billing-service/billing-service.csproj

cd frontend
npm test -- --watch=false
npm run build
```

Os testes do frontend verificam a inicialização da aplicação, a marca exibida, a consolidação de produtos repetidos, a validação de saldo e a tradução dos status.

## Endpoints principais

| Método | Endpoint | Descrição |
|---|---|---|
| `GET` | Inventory `/api/products` | Lista produtos |
| `POST` | Inventory `/api/products` | Cadastra produto |
| `POST` | Inventory `/api/products/decrease-stock` | Baixa lote idempotente |
| `GET` | Billing `/api/invoices` | Lista notas e itens |
| `POST` | Billing `/api/invoices` | Cria nota aberta |
| `POST` | Billing `/api/invoices/{id}/print` | Baixa estoque e fecha a nota |


## Estrutura

```text
Korp_Teste_Douglas/
├── frontend/             # Angular
├── inventory-service/    # ASP.NET Core + inventory_db
├── billing-service/      # ASP.NET Core + billing_db
└── README.md
```

## Autor

Douglas Santos
