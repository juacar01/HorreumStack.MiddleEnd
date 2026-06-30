using HorreumStack.Domain.Enums;

namespace HorreumStack.MiddleEnd.Core.Features.Ubicaciones
{
    public class UbicacionResponse
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public Guid AlmacenId { get; set; } = Guid.Empty;
        public Guid? ubicacionTipoId { get; set; }
        public Guid UbicacionPadreId { get; set; } = Guid.Empty;
        public UbicacionResponse UbicacionPadre { get; set; } = null!;
        public Boolean IsPrincipal { get; set; } = false;
        public UbicacionStatus Status { get; set; } = UbicacionStatus.Active;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }

    }
}