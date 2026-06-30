namespace HorreumStack.MiddleEnd.Core.Features.Items;

public interface IItemService
{
    Task<List<ItemResponse>> GetAllAsync();
    Task<List<ItemResponse>> GetAllByProyectoIdAsync(Guid proyectoId);
    Task<ItemPaginationResponse> GetPaginatedByProyectoIdAsync(Guid proyectoId, int pageIndex, int pageSize, string? search);
    Task<ItemResponse> GetByIdAsync(Guid id);
    Task<ItemResponse> CreateAsync(ItemDto model);
    Task<ItemResponse> UpdateAsync(Guid id, ItemDto model);
    Task<bool> DeleteAsync(Guid id);
    Task<int> ImportItemsAsync(Guid proyectoId, Stream fileStream, string fileExtension, string createdBy);
}
