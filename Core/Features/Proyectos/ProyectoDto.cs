using HorreumStack.Domain.Entities;

namespace HorreumStack.MiddleEnd.Core.Features.Proyectos;

public class ProyectoDto
{

    public Guid? Id { get; set; } = Guid.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public Boolean? IsMine { get; set; } = true;
    public Guid? OwnerId { get; set; } = Guid.Empty;
    public String? OwnerName { get; set; } = string.Empty;

    public virtual ICollection<ProyectoUser> ProyectoUsers { get; set; } = new List<ProyectoUser>();
    public virtual ICollection<Almacen> Almacenes { get; set; } = new List<Almacen>();

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

    /*
     public required string Codigo { get; set; }
        public required string Nombre { get; set; }

        public Boolean IsPrincipal { get; set; } = false;
        public ProyectoStatus Status { get; set; } = ProyectoStatus.Active;
        public virtual ICollection<ProyectoUser> ProyectoUsers { get; set; } = new List<ProyectoUser>();
        public virtual ICollection<Almacen> Almacenes { get; set; } = new List<Almacen>();

    */

}
