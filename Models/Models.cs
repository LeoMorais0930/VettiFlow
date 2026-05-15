namespace VettiFlow.Api.Models;

public class Blueprint
{
    public int Id { get; set; }
    public string Code { get; set; } = "";        // ex: "105-141"
    public string Name { get; set; } = "";        // ex: "Central Vetti Smart"
    public int DefaultBatchSize { get; set; } = 100;
    public List<BlueprintStage> Stages { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BlueprintStage
{
    public int Id { get; set; }
    public int BlueprintId { get; set; }
    public Blueprint Blueprint { get; set; } = null!;
    public int Order { get; set; }               // posição na linha (0-based)
    public string Name { get; set; } = "";       // ex: "Gravadora"
}

public class ProductionOrder
{
    public int Id { get; set; }
    public int BlueprintId { get; set; }
    public Blueprint Blueprint { get; set; } = null!;
    public string Label { get; set; } = "";
    public int TotalQty { get; set; }
    public bool IsHighPriority { get; set; }
    public int CurrentStageIndex { get; set; } = 0;
    public bool IsCompleted { get; set; }
    public string ComponentCodes { get; set; } = "";
    public List<KitComponentStatus> KitStatuses { get; set; } = []; // NOVO: Status por item
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<OrderStageProgress> Progress { get; set; } = [];
}

public class KitComponentStatus
{
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; } = 1; // NOVO: Quantidade por item
    public int CurrentStageIndex { get; set; } = 0;
    public bool IsCompleted { get; set; }
}

// Log auditável — uma linha por avanço de etapa
public class OrderStageProgress
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public ProductionOrder Order { get; set; } = null!;
    public int StageIndex { get; set; }
    public string StageName { get; set; } = "";            // snapshot do nome
    public DateTime AdvancedAt { get; set; } = DateTime.UtcNow;
    public string? AdvancedBy { get; set; }                // dispositivo/usuário (opcional)
}
