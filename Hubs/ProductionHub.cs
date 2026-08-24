using Microsoft.AspNetCore.SignalR;

namespace VettiFlow.Api.Hubs;

public class ProductionHub : Hub
{
}

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
