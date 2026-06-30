namespace HorreumStack.MiddleEnd.Core.Features.Items;

public class ItemPaginationResponse
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<ItemResponse> Data { get; set; } = new List<ItemResponse>();
}
