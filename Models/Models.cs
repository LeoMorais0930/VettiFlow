namespace VettiFlow.Api.Models;

public class Blueprint
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int DefaultBatchSize { get; set; } = 100;
    public List<BlueprintStage> Stages { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BlueprintStage
{
    public int Id { get; set; }
    public int BlueprintId { get; set; }
    public Blueprint Blueprint { get; set; } = null!;
    public int Order { get; set; }
    public string Name { get; set; } = "";
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
    public List<KitComponentStatus> KitStatuses { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<OrderStageProgress> Progress { get; set; } = [];
}

public class KitComponentStatus
{
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public int CurrentStageIndex { get; set; } = 0;
    public bool IsCompleted { get; set; }
}

public class OrderStageProgress
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public ProductionOrder Order { get; set; } = null!;
    public int StageIndex { get; set; }
    public string StageName { get; set; } = "";
    public DateTime AdvancedAt { get; set; } = DateTime.UtcNow;
    public string? AdvancedBy { get; set; }
}
