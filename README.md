# VETTI Flow — Backend Protótipo

## Pré-requisitos
- .NET 8 SDK
- PostgreSQL 15+ rodando localmente

## Setup em 3 comandos

```bash
# 1. Crie o banco
psql -U postgres -c "CREATE DATABASE vettiflow;"

# 2. Ajuste a senha em appsettings.json
#    "Password=SUA_SENHA_AQUI"  →  sua senha do postgres

# 3. Rode
dotnet run
```

API disponível em: `http://localhost:5000`
SignalR Hub em:    `http://localhost:5000/hubs/production`

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET    | /api/blueprints | Lista todos os produtos |
| POST   | /api/blueprints | Cadastra produto + etapas |
| GET    | /api/orders | Orders abertas (para a TV) |
| POST   | /api/orders | Lança novo pedido |
| POST   | /api/orders/{id}/advance | **Botão PRÓXIMA ETAPA** — grava log + notifica TV via SignalR |
| GET    | /api/orders/{id}/history | Histórico completo de avanços de um pedido |

> O app Flutter deve enviar o header `X-Device-Id: <nome-do-dispositivo>` no advance  
> para identificar quem clicou (aparecerá no log de auditoria).

## Teste rápido (curl)

```bash
# Criar um pedido
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"blueprintId":1,"label":"Pedido Argentina - Lote 01","totalQty":300,"isHighPriority":true}'

# Avançar etapa (simula clique do gestor)
curl -X POST http://localhost:5000/api/orders/1/advance
```

## Evento SignalR que a TV escuta

```json
// Canal: "OrderUpdated"
{
  "orderId": 1,
  "label": "Pedido Argentina - Lote 01",
  "totalQty": 300,
  "currentStageIndex": 1,
  "totalStages": 4,
  "isHighPriority": true,
  "isCompleted": false,
  "stageNames": ["Gravadora", "Teste 01", "Fechamento", "Expedição"]
}
```

## Próximo passo
Conectar o Flutter (TV Dashboard e App do Gestor) neste backend.
