using System;
using HorreumStack.MiddleEnd.Core.Features.Proyectos;

namespace HorreumStack.MiddleEnd.Core.Features.UnidadMedidas;

public class UnidadMedidaResponse
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public Guid? ProyectoId { get; set; }
    public ProyectoResponse? Proyecto { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
}
