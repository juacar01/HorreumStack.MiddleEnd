using HorreumStack.MiddleEnd.Core.Features.Proyectos;
using HorreumStack.MiddleEnd.Core.Features.ItemTipos;
using HorreumStack.MiddleEnd.Core.Features.UnidadMedidas;

namespace HorreumStack.MiddleEnd.Core.Features.Items;

public class ItemResponse
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
    public Guid ProyectoId { get; set; }
    public ProyectoResponse? Proyecto { get; set; }
    public Guid? ItemTipoId { get; set; }
    public ItemTipoResponse? ItemTipo { get; set; }
    public Guid? UnidadMedidaId { get; set; }
    public UnidadMedidaResponse? UnidadMedida { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
}
