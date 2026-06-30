using System;

namespace HorreumStack.MiddleEnd.Core.Features.Inventarios;

public class AlmacenItemStockResponse
{
    public Guid ItemId { get; set; }
    public string ItemNombre { get; set; } = string.Empty;
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemTipoNombre { get; set; } = string.Empty;
    public string UnidadMedidaNombre { get; set; } = string.Empty;
    public decimal CantidadTotal { get; set; }
    public string Imagen { get; set; } = string.Empty;
}
