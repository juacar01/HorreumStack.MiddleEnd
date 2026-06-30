namespace HorreumStack.MiddleEnd.Core.Features.UbicacionesTipos;

public class UbicacionTipoPaginationResponse
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<UbicacionTipoResponse> Data { get; set; } = new List<UbicacionTipoResponse>();
}
