using HorreumStack.MiddleEnd.Core.Features.Items;

namespace HorreumStack.MiddleEnd.Core.Features.Inventarios;

public class InventarioMovimientoResponse
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public ItemResponse Item { get; set; } = null!;
    public Guid? UbicacionOrigenId { get; set; }
    public string UbicacionOrigenNombre { get; set; } = string.Empty;
    public Guid? UbicacionDestinoId { get; set; }
    public string UbicacionDestinoNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
