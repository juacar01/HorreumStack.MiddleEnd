namespace HorreumStack.MiddleEnd.Core.Features.UbicacionesTipos;

public interface IUbicacionTipoService
{
    Task<List<UbicacionTipoResponse>> GetAllAsync();
    Task<List<UbicacionTipoResponse>> GetAllByProyectoIdAsync(Guid proyectoId);
    Task<UbicacionTipoPaginationResponse> GetPaginatedByProyectoIdAsync(Guid proyectoId, int pageIndex, int pageSize, string? search);
    Task<List<UbicacionTipoComboResponse>> GetAllComboByProyectoIdAsync(Guid proyectoId);
    Task<UbicacionTipoResponse> GetByIdAsync(Guid id);
    Task<UbicacionTipoResponse> CreateAsync(UbicacionTipoDto model);
    Task<UbicacionTipoResponse> UpdateAsync(Guid id, UbicacionTipoDto model);
    Task<bool> DeleteAsync(Guid id);
}
