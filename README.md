# Korp - Sistema de Emissão de Notas Fiscais

Projeto técnico desenvolvido para o processo seletivo da **Korp**.

A aplicação implementa cadastro de produtos, controle de estoque, criação e impressão de notas fiscais utilizando **Angular**, **C# / ASP.NET Core**, **microsserviços** e **MySQL**.

---

## Arquitetura

```text
Angular Frontend
      │
      ├──────────────► Inventory Service
      │                    │
      │                    ▼
      │               inventory_db
      │
      └──────────────► Billing Service
                           │
                           ├──── HTTP ────► Inventory Service
                           │
                           ▼
                      billing_db
```

O sistema possui dois microsserviços:

- **Inventory Service**: cadastro de produtos e controle de estoque.
- **Billing Service**: criação, consulta, impressão e fechamento de notas fiscais.

---

## Tecnologias

### Frontend
- Angular
- TypeScript
- RxJS
- Angular Router
- Angular Forms
- Angular HttpClient
- SCSS

### Backend
- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- LINQ
- HttpClient

### Banco
- MySQL
- Entity Framework Core Migrations

---

## Funcionalidades

### Produtos
- Cadastro com código, descrição e saldo.
- Código único.
- Persistência em MySQL.
- Validação de dados.
- Controle e baixa de estoque.

### Notas Fiscais
- Numeração sequencial.
- Status inicial `Open`.
- Inclusão de múltiplos produtos.
- Quantidade por produto.
- Persistência no banco.
- Impressão e alteração para `Closed`.

### Impressão
Ao imprimir uma nota:

1. O Billing Service verifica se ela está `Open`.
2. Solicita a baixa dos itens ao Inventory Service.
3. O estoque é validado e atualizado em transação.
4. Após sucesso, a nota é alterada para `Closed`.
5. Notas fechadas não podem ser impressas novamente.

---

## Tratamento de falhas

Caso o Inventory Service esteja indisponível durante a impressão:

- o Billing Service trata a falha de comunicação;
- retorna `503 Service Unavailable`;
- a nota permanece `Open`;
- o frontend apresenta feedback ao usuário;
- após a recuperação do serviço, a operação pode ser tentada novamente.

Também são utilizados:

- `400 Bad Request`
- `404 Not Found`
- `409 Conflict`
- `503 Service Unavailable`

---

## Transação de estoque

A baixa de múltiplos produtos utiliza transação no banco.

Caso algum item não exista ou não possua saldo suficiente, a operação não é confirmada.

```text
Begin Transaction
      ↓
Validar itens
      ↓
Atualizar saldos
      ↓
Commit

Falha → Rollback
```

---

## Angular

### Ciclo de vida

Foi utilizado principalmente:

```typescript
ngOnInit()
```

para carregar produtos, notas e dados do dashboard na inicialização dos componentes.

### RxJS

O RxJS é utilizado através dos `Observable` retornados pelo `HttpClient`.

Exemplo:

```typescript
this.productService.getAll().subscribe({
  next: products => this.products = products,
  error: () => this.errorMessage = 'Erro ao carregar produtos.'
});
```

---

## LINQ

O backend utiliza LINQ com Entity Framework Core.

Principais métodos:

- `FirstOrDefaultAsync`
- `AnyAsync`
- `OrderBy`
- `MaxAsync`
- `Select`
- `ToListAsync`

Exemplos de uso incluem busca de produtos, validação de códigos duplicados, ordenação das notas e geração da numeração sequencial.

---

## Componentes visuais

Não foi utilizada biblioteca externa como Angular Material ou Bootstrap.

A interface foi construída com:

- Angular Templates
- HTML
- SCSS

Incluindo dashboard, sidebar, formulários, tabelas, badges de status e layout responsivo.

---

## Estrutura

```text
Korp_Teste_Douglas/
├── frontend/
├── inventory-service/
├── billing-service/
├── .gitignore
└── README.md
```

---

## Configuração

Os arquivos reais `appsettings.json` não são versionados.

Cada serviço possui:

```text
appsettings.example.json
```

Exemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=inventory_db;user=YOUR_USER;password=YOUR_PASSWORD;"
  }
}
```

No Billing Service, utilize:

```text
database=billing_db
```

---

## Como executar

### Pré-requisitos

- .NET SDK 10
- Node.js
- npm
- Angular CLI
- MySQL 8+

### Inventory Service

```bash
cd inventory-service
dotnet restore
dotnet ef database update
dotnet run
```

```text
http://localhost:5277
```

### Billing Service

```bash
cd billing-service
dotnet restore
dotnet ef database update
dotnet run
```

```text
http://localhost:5227
```

### Frontend

```bash
cd frontend
npm install
ng serve
```

```text
http://localhost:4200
```

---

## Portas

| Aplicação | Endereço |
|---|---|
| Angular | `http://localhost:4200` |
| Inventory Service | `http://localhost:5277` |
| Billing Service | `http://localhost:5227` |

---

## Requisitos atendidos

- ✅ Angular
- ✅ Cadastro de produtos
- ✅ Cadastro de notas fiscais
- ✅ Numeração sequencial
- ✅ Status Open / Closed
- ✅ Múltiplos produtos
- ✅ Botão de impressão
- ✅ Indicador de processamento
- ✅ Atualização do estoque
- ✅ Dois microsserviços
- ✅ Persistência real em MySQL
- ✅ Tratamento de falha entre serviços
- ✅ Feedback de erro
- ✅ RxJS
- ✅ LINQ
- ✅ Dashboard
- ✅ Interface responsiva

---

## Autor

**Douglas Santos**

Projeto desenvolvido para o teste técnico da Korp.