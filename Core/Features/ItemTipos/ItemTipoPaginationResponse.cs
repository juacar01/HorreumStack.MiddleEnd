namespace HorreumStack.MiddleEnd.Core.Features.ItemTipos;

public class ItemTipoPaginationResponse
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<ItemTipoResponse> Data { get; set; } = new List<ItemTipoResponse>();
}
