namespace HorreumStack.MiddleEnd.Core.Features.ItemTipos;

public interface IItemTipoService
{
    Task<List<ItemTipoResponse>> GetAllAsync();
    Task<List<ItemTipoResponse>> GetAllByProyectoIdAsync(Guid proyectoId);
    Task<ItemTipoPaginationResponse> GetPaginatedByProyectoIdAsync(Guid proyectoId, int pageIndex, int pageSize, string? search);
    Task<List<ItemTipoComboResponse>> GetAllComboByProyectoIdAsync(Guid proyectoId);
    Task<ItemTipoResponse> GetByIdAsync(Guid id);
    Task<ItemTipoResponse> CreateAsync(ItemTipoDto model);
    Task<ItemTipoResponse> UpdateAsync(Guid id, ItemTipoDto model);
    Task<bool> DeleteAsync(Guid id);
}
