using HorreumStack.MiddleEnd.Core.Features.Users;

namespace HorreumStack.MiddleEnd.Core.Features.Proyectos;

public class ProyectoResponse
{

    public Guid? Id { get; set; } = Guid.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public Boolean? IsMine { get; set; } = false;
    public Boolean IsPrincipal { get; set; } = false;
    public Guid? OwnerId { get; set; } = Guid.Empty;
    public virtual UserVm? Owner { get; set; } = null!;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
}