using HorreumStack.Domain.Entities;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public class AlmacenDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }

}