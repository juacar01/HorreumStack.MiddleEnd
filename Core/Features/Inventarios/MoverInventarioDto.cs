namespace HorreumStack.MiddleEnd.Core.Features.Inventarios;

public class MoverInventarioDto
{
    public Guid ItemId { get; set; }
    public decimal Cantidad { get; set; }
    public Guid UbicacionDestinoId { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}
