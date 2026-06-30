using HorreumStack.MiddleEnd.Core.Features.Almacenes;

namespace HorreumStack.MiddleEnd.Core.Features.Ubicaciones;

public class UbicacionDetailResponse : UbicacionResponse
{
    public virtual AlmacenResponse Almacen { get; set; } = null!;
}