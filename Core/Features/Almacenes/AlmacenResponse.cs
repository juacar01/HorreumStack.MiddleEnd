using HorreumStack.Domain.Enums;
using HorreumStack.Domain.Entities;
using HorreumStack.MiddleEnd.Core.Features.Proyectos;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public class AlmacenResponse
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool IsPrincipal { get; set; } = false;
    public Guid ProyectoId { get; set; } = Guid.Empty;
    public ProyectoResponse Proyecto { get; set; } = null!;
    public AlmacenStatus Status { get; set; } = AlmacenStatus.Active;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

}

