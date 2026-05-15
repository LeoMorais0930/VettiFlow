using Microsoft.AspNetCore.SignalR;

namespace VettiFlow.Api.Hubs;

public class ProductionHub : Hub
{
    // Clientes escutam "OrderUpdated" — a TV recebe isso automaticamente
    // Não precisamos de métodos aqui no protótipo;
    // o servidor envia via IHubContext nos endpoints.
}

// DTO enviado ao atualizar uma order
public record OrderUpdateDto(
    int OrderId,
    string Label,
    int TotalQty,
    int CurrentStageIndex,
    int TotalStages,
    bool IsHighPriority,
    bool IsCompleted,
    string[] StageNames
);
