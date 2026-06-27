using HorreumStack.Domain.Entities;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public interface IAlmacenService
{
    Task<List<AlmacenDto>> GetAllAsync();
    Task<AlmacenDto> GetByIdAsync(Guid id);
    Task<AlmacenDto> GetByCodeAsync(string code);
    Task<List<AlmacenDto>> GetListByUserIdAsync(Guid userId);
    Task<AlmacenDto> CreateAsync(AlmacenDto model, Guid userId);
    Task<AlmacenDto> UpdateAsync(Guid id, AlmacenDto model);
    Task<bool> DeleteAsync(Guid id);
}