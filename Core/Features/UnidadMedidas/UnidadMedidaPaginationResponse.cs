using System.Collections.Generic;

namespace HorreumStack.MiddleEnd.Core.Features.UnidadMedidas;

public class UnidadMedidaPaginationResponse
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<UnidadMedidaResponse> Data { get; set; } = new List<UnidadMedidaResponse>();
}
