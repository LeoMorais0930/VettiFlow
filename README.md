# VETTI Flow — API

Backend desenvolvido em .NET 8 utilizando Minimal APIs para o ecossistema de monitoramento de produção VETTI.

## 🚀 Tecnologias
- **.NET 8**
- **SignalR** (Comunicação em tempo real)
- **JSON Store** (Persistência em arquivo local para prototipagem rápida)
- **Swagger/OpenAPI** (Documentação da API)

## 🛠️ Configuração e Execução

### Pré-requisitos
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download/dotnet/8.0)

### Comandos Principais
```bash
# Restaurar dependências
dotnet restore

# Executar o projeto (escutando em http://0.0.0.0:5000)
dotnet run
```

O servidor está configurado para aceitar conexões de qualquer IP na rede local, facilitando o acesso via dispositivos móveis e TVs.

## 📡 Endpoints (Swagger)
Ao rodar o projeto, a documentação interativa estará disponível em:
`http://localhost:5000/` ou `http://<seu-ip-local>:5000/`

## 📡 SignalR Hub
- **Caminho**: `/hubs/production`
- **Eventos enviados**:
    - `RefreshAll`: Notifica os clientes para recarregar todos os dados.
    - `OrderUpdated`: Envia um objeto `OrderUpdateDto` quando um pedido é alterado.

---
Desenvolvido para **VETTI — Segurança e Tecnologia**.
