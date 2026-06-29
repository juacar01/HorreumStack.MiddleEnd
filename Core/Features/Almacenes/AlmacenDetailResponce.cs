using HorreumStack.MiddleEnd.Core.Features.Ubicaciones;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public class AlmacenDetailResponse : AlmacenResponse
{
    public virtual List<UbicacionResponse> Ubicaciones { get; set; } = new List<UbicacionResponse>();
}
