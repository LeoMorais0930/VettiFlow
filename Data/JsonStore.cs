using System.Text.Json;
using VettiFlow.Api.Models;

namespace VettiFlow.Api.Data;

public class JsonStore
{
    private static readonly string FilePath =
        Path.Combine(AppContext.BaseDirectory, "vettiflow_data.json");

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private StoreData _data = new();

    public List<Blueprint>          Blueprints => _data.Blueprints;
    public List<ProductionOrder>    Orders     => _data.Orders;
    public List<OrderStageProgress> Progress   => _data.Progress;

    public void Load()
    {
        if (!File.Exists(FilePath)) { Seed(); Save(); return; }
        var json = File.ReadAllText(FilePath);
        _data = JsonSerializer.Deserialize<StoreData>(json, JsonOpts) ?? new StoreData();
        if (!_data.Blueprints.Any()) { Seed(); Save(); }
    }

    public void Save()
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(_data, JsonOpts));
    }

    public int NextBlueprintId()  => (_data.Blueprints.MaxBy(x => x.Id)?.Id  ?? 0) + 1;
    public int NextOrderId()      => (_data.Orders    .MaxBy(x => x.Id)?.Id  ?? 0) + 1;
    public int NextProgressId()   => (_data.Progress  .MaxBy(x => x.Id)?.Id  ?? 0) + 1;
    public int NextStageId()      =>
        (_data.Blueprints.SelectMany(b => b.Stages).MaxBy(x => x.Id)?.Id ?? 0) + 1;

    public void ClearCompletedOrders()
    {
        _data.Orders.RemoveAll(o => o.IsCompleted);
        Save();
    }

    public void ClearAuditLog()
    {
        try
        {
            string logPath = Path.Combine(AppContext.BaseDirectory, "audit_log.txt");
            if (File.Exists(logPath)) File.Delete(logPath);
        }
        catch { }
    }

    private void Seed()
    {
        _data.Blueprints.Add(new Blueprint
        {
            Id = 1, Code = "105-141", Name = "Central Vetti Smart",
            DefaultBatchSize = 100, CreatedAt = DateTime.UtcNow,
            Stages =
            [
                new BlueprintStage { Id = 1, BlueprintId = 1, Order = 0, Name = "Gravadora"  },
                new BlueprintStage { Id = 2, BlueprintId = 1, Order = 1, Name = "Teste 01"   },
                new BlueprintStage { Id = 3, BlueprintId = 1, Order = 2, Name = "Fechamento" },
                new BlueprintStage { Id = 4, BlueprintId = 1, Order = 3, Name = "Expedição"  },
            ]
        });
    }
}

public class StoreData
{
    public List<Blueprint>          Blueprints { get; set; } = [];
    public List<ProductionOrder>    Orders     { get; set; } = [];
    public List<OrderStageProgress> Progress   { get; set; } = [];
}
