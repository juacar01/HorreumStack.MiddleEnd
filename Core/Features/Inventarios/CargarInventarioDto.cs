namespace HorreumStack.MiddleEnd.Core.Features.Inventarios;

public class CargarInventarioDto
{
    public Guid ItemId { get; set; }
    public decimal Cantidad { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}
