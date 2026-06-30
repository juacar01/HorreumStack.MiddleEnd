namespace HorreumStack.MiddleEnd.Core.Features.Ubicaciones;

public interface IUbicacionService
{
    Task<List<UbicacionResponse>> GetAllAsync();
    Task<List<UbicacionResponse>> GetAllByAlmacenIdAsync(Guid almacenId);
    Task<UbicacionDetailResponse> GetByIdAsync(Guid id);
    Task<UbicacionResponse> CreateAsync(UbicacionDto model);
    Task<UbicacionResponse> UpdateAsync(Guid id, UbicacionDto model);
    Task<bool> DeleteAsync(Guid id);
}