# VETTI Flow — API

Backend desenvolvido em .NET utilizando Minimal APIs para o ecossistema de monitoramento de produção VETTI.

## 🚀 Tecnologias
- **.NET**
- **SignalR** (Comunicação em tempo real)
- **JSON Store** (Persistência em arquivo local para prototipagem rápida)
- **Swagger/OpenAPI** (Documentação da API)

## 🛠️ Configuração e Execução

### Pré-requisitos
- .NET SDK compatível com o `TargetFramework` do projeto

### Comandos Principais
```bash
# Restaurar dependências
dotnet restore

# Executar o projeto no servidor da empresa
dotnet run
```

Por padrão, a API escuta em:

`http://10.36.0.4:5000`

Para gerar o pacote de servidor:

```powershell
.\publish-server.ps1
```

Depois, copie o ZIP gerado em `..\dist\VettiFlow.Api-server-win-x64-10.36.0.4.zip` para o servidor, extraia e execute `run-vettiflow-server.bat` como administrador.

## 📡 Endpoints (Swagger)
Ao rodar o projeto, a documentação interativa estará disponível em:
`http://10.36.0.4:5000/swagger`

## 📡 SignalR Hub
- **Caminho**: `/hubs/production`
- **Eventos enviados**:
    - `RefreshAll`: Notifica os clientes para recarregar todos os dados.
    - `OrderUpdated`: Envia um objeto `OrderUpdateDto` quando um pedido é alterado.

---
Desenvolvido para **VETTI — Segurança e Tecnologia**.
