# VettiFlow API

[Portugues](#portugues) | [English](#english)

## Portugues

Backend em .NET para apoiar o fluxo de producao da VETTI, centralizando cadastros de produtos, ordens de producao, etapas, historico e atualizacoes em tempo real para os frontends operacionais.

### O problema

Processos de producao acompanhados por planilhas, controles manuais ou comunicacao informal dificultam rastreabilidade, priorizacao e visao em tempo real do que esta em cada etapa.

### A solucao

Esta API organiza o fluxo em entidades simples de operacao:

- Produtos/modelos com etapas configuraveis.
- Ordens de producao com quantidade, prioridade e componentes.
- Avanco de etapas com historico e auditoria local.
- Notificacoes em tempo real via SignalR para atualizar paineis e apps conectados.
- Swagger/OpenAPI para facilitar testes, documentacao e integracao.
- Servico de arquivos estaticos para publicar o frontend Flutter Web junto da API.

### Stack

- .NET / ASP.NET Core Minimal APIs
- SignalR
- Swagger / OpenAPI
- Persistencia local em JSON para a primeira versao interna
- Scripts PowerShell para empacotamento e deploy em servidor Windows

### Decisoes tecnicas

- Minimal APIs para manter o backend direto e simples de evoluir.
- `JsonStore` centralizado para reduzir dependencia de infraestrutura durante a fase inicial.
- Hub SignalR em `/hubs/production` para sincronizar telas sem refresh manual.
- Configuracao de URL por `appsettings.json` ou variavel de ambiente.

### Como rodar

Pre-requisito: .NET SDK compativel com o `TargetFramework` do projeto.

```bash
dotnet restore
dotnet run
```

Por padrao, a API usa a URL configurada em `VettiFlow:ListenUrl`. No ambiente interno original, a URL utilizada era:

```text
http://10.36.0.4:5000
```

Swagger:

```text
http://localhost:5000/swagger
```

### Empacotamento para servidor

```powershell
.\publish-server.ps1
```

O script gera um pacote em `..\dist\`. Depois de extrair no servidor, execute `run-vettiflow-server.bat` conforme a configuracao do ambiente.

### Endpoints principais

- `GET /api/blueprints`
- `POST /api/blueprints`
- `GET /api/orders`
- `POST /api/orders`
- `POST /api/orders/{id}/advance`
- `GET /api/orders/{id}/history`
- `GET /swagger`

## English

.NET backend for VETTI's production flow, centralizing product blueprints, production orders, stages, history, and real-time updates for operational frontends.

### Problem

Production processes tracked through spreadsheets, manual notes, or informal communication make it harder to keep traceability, priorities, and real-time visibility across stages.

### Solution

This API models the operation with straightforward building blocks:

- Product blueprints with configurable stages.
- Production orders with quantity, priority, and components.
- Stage progression with local audit history.
- Real-time SignalR notifications for connected dashboards and apps.
- Swagger/OpenAPI for testing, documentation, and integration.
- Static file hosting to publish the Flutter Web frontend together with the API.

### Tech Stack

- .NET / ASP.NET Core Minimal APIs
- SignalR
- Swagger / OpenAPI
- Local JSON persistence for the first internal version
- PowerShell scripts for packaging and Windows Server deployment

### Technical Decisions

- Minimal APIs keep the backend small and easy to evolve.
- A centralized `JsonStore` reduces infrastructure dependency in the first version.
- SignalR hub at `/hubs/production` keeps connected screens synchronized.
- Listen URL can be configured through `appsettings.json` or environment variables.

### Running Locally

Requirement: .NET SDK compatible with the project `TargetFramework`.

```bash
dotnet restore
dotnet run
```

The API uses the configured `VettiFlow:ListenUrl`. The original internal environment used:

```text
http://10.36.0.4:5000
```

Swagger:

```text
http://localhost:5000/swagger
```

### Server Package

```powershell
.\publish-server.ps1
```

The script creates a package under `..\dist\`. After extracting it on the server, run `run-vettiflow-server.bat` according to the environment configuration.
