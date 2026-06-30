using HorreumStack.MiddleEnd.Core.Features.Items;

namespace HorreumStack.MiddleEnd.Core.Features.Inventarios;

public class InventarioResponse
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public ItemResponse Item { get; set; } = null!;
    public Guid UbicacionId { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime CreatedAt { get; set; }
}
