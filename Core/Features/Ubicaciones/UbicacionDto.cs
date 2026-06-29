using HorreumStack.Domain.Enums;

namespace HorreumStack.MiddleEnd.Core.Features.Ubicaciones;

public class UbicacionDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Codigo { get; set; } = string.Empty;
    public Guid AlmacenId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public UbicacionStatus Status { get; set; } = UbicacionStatus.Active;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? LastModifiedBy { get; set; }
}