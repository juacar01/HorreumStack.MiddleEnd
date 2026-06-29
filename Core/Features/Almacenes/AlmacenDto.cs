using HorreumStack.Domain.Entities;
using HorreumStack.MiddleEnd.Core.Features.Proyectos;
using HorreumStack.MiddleEnd.Core.Features.Ubicaciones;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public class AlmacenDto
{
    public Guid? Id { get; set; } = Guid.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; } = string.Empty;
    public string? Direccion { get; set; } = string.Empty;
    public Boolean? IsPrincipal { get; set; } = false;

    public Guid ProyectoId { get; set; }
    public ProyectoDto? Proyecto { get; set; } = null;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    public List<UbicacionDto> Ubicaciones { get; set; } = new List<UbicacionDto>();
}

/*
using HorreumStack.Domain.Entities.Common;
using HorreumStack.Domain.Enums;

namespace HorreumStack.Domain.Entities;

public class Almacen : EntityBase
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; } = string.Empty;
    public string? Direccion { get; set; } = string.Empty;
    public bool IsPrincipal { get; set; } = false;
    public AlmacenStatus Status { get; set; } = AlmacenStatus.Active;
    public Guid? ProyectoId { get; set; }
    public virtual Proyecto? Proyecto { get; set; }
    public virtual ICollection<Ubicacion> Ubicaciones { get; set; } = new List<Ubicacion>();

}

*/