# Korp — Sistema de Emissão de Notas Fiscais

Projeto desenvolvido para o teste técnico da **Korp**.

Sistema para cadastro de produtos, controle de estoque e emissão de notas fiscais, utilizando **Angular**, **C# / ASP.NET Core**, arquitetura de **microsserviços** e **MySQL**.

## Arquitetura

```text
Angular (4200)
   │
   ├── HTTP ──> Inventory Service (5277) ──> inventory_db
   │
   └── HTTP ──> Billing Service   (5227) ──> billing_db
                         │
                         └── HTTP ──> Inventory Service
```

- **Inventory Service:** cadastro de produtos e controle de estoque.
- **Billing Service:** criação, consulta, impressão e fechamento de notas fiscais.
- **Frontend:** interface Angular para produtos, notas fiscais e dashboard.
- **Banco de dados:** cada microsserviço possui sua própria base MySQL.

## Funcionalidades

- Cadastro de produtos com código único, descrição e saldo.
- Criação de notas fiscais com múltiplos produtos.
- Numeração sequencial das notas.
- Status `Open` e `Closed`.
- Impressão de notas abertas.
- Atualização automática do estoque após impressão.
- Bloqueio de impressão de notas já fechadas.
- Validação de estoque insuficiente.
- Indicador de processamento durante a impressão.
- Dashboard com resumo de produtos e notas.
- Interface responsiva.

## Concorrência e Idempotência

A baixa de estoque é realizada de forma transacional e protegida contra operações simultâneas, evitando saldo negativo.

As operações de estoque utilizam uma chave de idempotência:

```text
billing-invoice-{id}
```

Isso impede que uma nova tentativa da mesma operação desconte o estoque mais de uma vez.

A numeração das notas também possui proteção contra criação simultânea, além de índice único no banco de dados.

## Tratamento de Falhas

Caso o **Inventory Service** esteja indisponível durante a impressão:

1. O Billing Service realiza uma nova tentativa;
2. Caso a falha continue, retorna `503 Service Unavailable`;
3. A nota permanece aberta;
4. O frontend apresenta uma mensagem ao usuário;
5. A operação pode ser realizada novamente após a recuperação do serviço.

Também são tratados:

- `400 Bad Request` — dados inválidos;
- `404 Not Found` — recurso não encontrado;
- `409 Conflict` — conflito de regra de negócio;
- `500 Internal Server Error` — erro inesperado;
- `503 Service Unavailable` — serviço dependente indisponível.

Exceções inesperadas são tratadas globalmente utilizando `ProblemDetails`.

## Tecnologias

### Frontend

- Angular 21
- TypeScript
- Angular Router
- Angular Forms
- Angular HttpClient
- RxJS 7
- SCSS
- Vitest

### Backend

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- LINQ
- HttpClientFactory

### Banco de Dados

- MySQL
- Entity Framework Core Migrations

Não foi utilizada biblioteca visual externa. A interface foi construída com Angular, HTML e SCSS.

## Angular e RxJS

O ciclo de vida `ngOnInit()` é utilizado para carregar os dados iniciais dos componentes.

O RxJS é utilizado nas requisições HTTP através de `Observable`, além de:

- `subscribe` para tratamento das respostas;
- `forkJoin` para requisições paralelas no Dashboard;
- `finalize` para controle dos indicadores de carregamento.

## LINQ

O backend utiliza LINQ junto ao Entity Framework Core para consultas e manipulação dos dados.

Principais operações utilizadas:

- `AnyAsync`
- `FirstOrDefaultAsync`
- `OrderBy`
- `GroupBy`
- `Select`
- `Sum`
- `ToListAsync`
- `AsNoTracking`
- `ExecuteUpdateAsync`

## Banco de Dados

São utilizadas duas bases MySQL:

```text
inventory_db
billing_db
```

A estrutura é gerenciada através das migrations do Entity Framework Core.

Os arquivos reais `appsettings.json` não são versionados. Cada serviço possui um:

```text
appsettings.example.json
```

para configuração do ambiente local.

## Como Executar

### Pré-requisitos

- .NET SDK 10
- Node.js
- npm
- MySQL 8+

### Inventory Service

```powershell
cd inventory-service
dotnet restore
dotnet ef database update
dotnet run
```

Disponível em:

```text
http://localhost:5277
```

### Billing Service

```powershell
cd billing-service
dotnet restore
dotnet ef database update
dotnet run
```

Disponível em:

```text
http://localhost:5227
```

### Frontend

```powershell
cd frontend
npm install
npm start
```

Acesse:

```text
http://localhost:4200
```

## Testes

```powershell
dotnet build inventory-service/inventory-service.csproj
dotnet build billing-service/billing-service.csproj

cd frontend
npm test -- --watch=false
npm run build
```

## Estrutura

```text
Korp_Teste_Douglas/
├── frontend/             # Angular
├── inventory-service/    # ASP.NET Core / inventory_db
├── billing-service/      # ASP.NET Core / billing_db
└── README.md
```

## Autor

**Douglas Santos**

Projeto desenvolvido para o teste técnico da Korp.