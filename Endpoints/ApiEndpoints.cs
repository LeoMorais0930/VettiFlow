using Microsoft.AspNetCore.SignalR;
using VettiFlow.Api.Data;
using VettiFlow.Api.Hubs;
using VettiFlow.Api.Models;
using System.IO;

namespace VettiFlow.Api.Endpoints;

public static class ApiEndpoints
{
    public static void Map(WebApplication app)
    {
        var api = app.MapGroup("/api");

        // Resolve Brasilia / São_Paulo timezone (Windows / IANA fallbacks)
        TimeZoneInfo? _brasilTz = null;
        try { _brasilTz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"); } catch { }
        if (_brasilTz == null)
        {
            try { _brasilTz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); } catch { }
        }

        DateTime GetBrasiliaTime() => _brasilTz == null
            ? DateTime.UtcNow
            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _brasilTz);

        DateTime? ToBrasilia(DateTime? dt)
        {
            if (!dt.HasValue) return null;
            try
            {
                var utc = DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
                return _brasilTz == null ? utc : TimeZoneInfo.ConvertTimeFromUtc(utc, _brasilTz);
            }
            catch { return dt; }
        }

        void AppendAuditLog(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audit_log.txt");
                File.AppendAllText(logPath, $"[{GetBrasiliaTime():dd/MM/yyyy HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch { }
        }

        // FUNÇÃO DE BROADCAST SIMPLIFICADA
        async Task NotifyClients(IHubContext<ProductionHub> hub)
        {
            // Enviamos um sinal simples que TODOS os apps entendem e forçam o recarregamento via HTTP
            await hub.Clients.All.SendAsync("RefreshAll");
        }

        api.MapGet("/blueprints", (JsonStore store) =>
            Results.Ok(store.Blueprints.Select(b => new {
                b.Id, b.Code, b.Name, b.DefaultBatchSize,
                Stages = b.Stages.OrderBy(s => s.Order)
            })));

        api.MapPost("/blueprints", async (JsonStore store, BlueprintRequest req, IHubContext<ProductionHub> hub) =>
        {
            var bp = new Blueprint {
                Id = store.NextBlueprintId(), Code = req.Code, Name = req.Name,
                DefaultBatchSize = req.DefaultBatchSize, CreatedAt = DateTime.UtcNow
            };
            for (int i = 0; i < req.Stages.Length; i++)
                bp.Stages.Add(new BlueprintStage {
                    Id = store.NextStageId() + i, BlueprintId = bp.Id, Order = i, Name = req.Stages[i]
                });
            store.Blueprints.Add(bp);
            store.Save();
            AppendAuditLog($"PRODUTO CADASTRADO: {bp.Code}");
            await NotifyClients(hub);
            return Results.Created($"/api/blueprints/{bp.Id}", bp);
        });

        api.MapDelete("/blueprints/{id:int}", async (int id, JsonStore store, IHubContext<ProductionHub> hub) =>
        {
            var bp = store.Blueprints.FirstOrDefault(b => b.Id == id);
            if (bp == null) return Results.NotFound();

            // Remove any orders that reference this blueprint
            var ordersToRemove = store.Orders.Where(o => o.BlueprintId == id).ToList();
            foreach (var o in ordersToRemove)
            {
                // remove related progress entries
                store.Progress.RemoveAll(p => p.OrderId == o.Id);
                store.Orders.Remove(o);
            }

            store.Blueprints.Remove(bp);
            store.Save();
            AppendAuditLog($"PRODUTO EXCLUÍDO: {bp.Code}");
            if (ordersToRemove.Count > 0) AppendAuditLog($"REMOVIDOS {ordersToRemove.Count} LOTES RELACIONADOS AO PRODUTO {bp.Code}");
            await NotifyClients(hub);
            return Results.NoContent();
        });

        api.MapGet("/orders", (JsonStore store) =>
        {
            var result = store.Orders.OrderByDescending(o => o.CreatedAt)
                .Select(o => {
                    var bp = store.Blueprints.FirstOrDefault(b => b.Id == o.BlueprintId);
                    return new {
                        o.Id, o.Label, o.TotalQty, o.IsHighPriority, o.CurrentStageIndex, o.IsCompleted,
                        o.ComponentCodes, o.KitStatuses,
                        CreatedAt = ToBrasilia(o.CreatedAt) ?? o.CreatedAt,
                        CompletedAt = ToBrasilia(o.CompletedAt) ?? o.CompletedAt,
                        Blueprint = bp == null ? null : new {
                            bp.Id, bp.Code, bp.Name, Stages = bp.Stages.OrderBy(s => s.Order)
                        }
                    };
                });
            return Results.Ok(result);
        });

        api.MapPost("/orders", async (JsonStore store, OrderRequest req, IHubContext<ProductionHub> hub) =>
        {
            var order = new ProductionOrder {
                Id = store.NextOrderId(), BlueprintId = req.BlueprintId, Label = req.Label,
                TotalQty = req.TotalQty, IsHighPriority = req.IsHighPriority,
                ComponentCodes = req.ComponentCodes ?? "", CreatedAt = DateTime.UtcNow
            };

            if (req.Components != null && req.Components.Length > 0)
            {
                foreach (var c in req.Components)
                {
                    var bp = store.Blueprints.FirstOrDefault(b => b.Id == c.BlueprintId);
                    order.KitStatuses.Add(new KitComponentStatus {
                        ProductCode = bp?.Code ?? "???",
                        ProductName = bp?.Name ?? "Produto",
                        Quantity = c.Quantity,
                        CurrentStageIndex = 0
                    });
                }
            }
            else if (!string.IsNullOrEmpty(req.ComponentCodes))
            {
                // Legado: parsing de string
                var codes = req.ComponentCodes.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var code in codes)
                {
                    var bp = store.Blueprints.FirstOrDefault(b => b.Code == code.Trim());
                    order.KitStatuses.Add(new KitComponentStatus {
                        ProductCode = code.Trim(),
                        ProductName = bp?.Name ?? "Produto",
                        Quantity = 1,
                        CurrentStageIndex = 0
                    });
                }
            }

            store.Orders.Add(order);
            store.Save();
            AppendAuditLog($"NOVO LOTE: {order.Label}");
            await NotifyClients(hub);
            return Results.Created($"/api/orders/{order.Id}", order);
        });

        api.MapPut("/orders/{id:int}", async (int id, OrderRequest req, JsonStore store, IHubContext<ProductionHub> hub) =>
        {
            var order = store.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return Results.NotFound();

            order.Label = req.Label;
            order.TotalQty = req.TotalQty;
            order.IsHighPriority = req.IsHighPriority;

            // Simples: recriamos os itens do kit se mudou (para fins de protótipo)
            if (req.Components != null)
            {
                order.KitStatuses.Clear();
                foreach (var c in req.Components)
                {
                    var bp = store.Blueprints.FirstOrDefault(b => b.Id == c.BlueprintId);
                    order.KitStatuses.Add(new KitComponentStatus {
                        ProductCode = bp?.Code ?? "???",
                        ProductName = bp?.Name ?? "Produto",
                        Quantity = c.Quantity,
                        CurrentStageIndex = 0
                    });
                }
            }

            store.Save();
            AppendAuditLog($"LOTE EDITADO: {order.Label}");
            await NotifyClients(hub);
            return Results.Ok(order);
        });

        api.MapDelete("/orders/{id:int}", async (int id, JsonStore store, IHubContext<ProductionHub> hub) =>
        {
            var order = store.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return Results.NotFound();

            store.Orders.Remove(order);
            store.Save();
            AppendAuditLog($"LOTE EXCLUÍDO: {order.Label}");
            await NotifyClients(hub);
            return Results.NoContent();
        });

        api.MapPost("/orders/{id:int}/advance",
            async (int id, string? componentIndex, HttpContext http, JsonStore store, IHubContext<ProductionHub> hub) =>
        {
            var order = store.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return Results.NotFound();

            var bp = store.Blueprints.FirstOrDefault(b => b.Id == order.BlueprintId);
            if (bp == null) return Results.NotFound();
            var stages = bp.Stages.OrderBy(s => s.Order).ToList();
            var advancedBy = http.Request.Headers["X-Device-Id"].FirstOrDefault() ?? "app";

            int? index = null;
            if (!string.IsNullOrEmpty(componentIndex) && int.TryParse(componentIndex, out int parsedIndex))
                index = parsedIndex;

            if (index.HasValue && index.Value < order.KitStatuses.Count)
            {
                var comp = order.KitStatuses[index.Value];
                if (comp.CurrentStageIndex < stages.Count - 1) comp.CurrentStageIndex++;
                else comp.IsCompleted = true;
                AppendAuditLog($"{advancedBy} AVANÇOU KIT ITEM: {comp.ProductCode} no lote {order.Label}");
            }
            else
            {
                if (order.CurrentStageIndex < stages.Count - 1) order.CurrentStageIndex++;
                else {
                    order.IsCompleted = true;
                    order.CompletedAt = DateTime.UtcNow;
                }
                AppendAuditLog($"{advancedBy} AVANÇOU LOTE: {order.Label}");
            }

            store.Save();
            await NotifyClients(hub); // NOTIFICAÇÃO GLOBAL
            return Results.Ok();
        });

        api.MapGet("/orders/{id:int}/history", (int id, JsonStore store) => {
            if (!store.Orders.Any(o => o.Id == id)) return Results.NotFound();
            var list = store.Progress.Where(p => p.OrderId == id).OrderBy(p => p.AdvancedAt)
                .Select(p => new {
                    p.Id,
                    p.OrderId,
                    p.StageIndex,
                    StageName = p.StageName,
                    AdvancedBy = p.AdvancedBy,
                    AdvancedAt = ToBrasilia(p.AdvancedAt) ?? p.AdvancedAt
                });
            return Results.Ok(list);
        });

        api.MapDelete("/orders/completed", async (JsonStore store, IHubContext<ProductionHub> hub) =>
        {
            store.ClearCompletedOrders();
            AppendAuditLog("LIMPEZA DE LOTES FINALIZADOS REALIZADA");
            await NotifyClients(hub);
            return Results.NoContent();
        });

        api.MapDelete("/logs", (JsonStore store) =>
        {
            store.ClearAuditLog();
            return Results.NoContent();
        });
    }
}

public record BlueprintRequest(string Code, string Name, int DefaultBatchSize, string[] Stages);
public record OrderRequest(int BlueprintId, string Label, int TotalQty, bool IsHighPriority, string? ComponentCodes, OrderComponentRequest[]? Components);
public record OrderComponentRequest(int BlueprintId, int Quantity);
