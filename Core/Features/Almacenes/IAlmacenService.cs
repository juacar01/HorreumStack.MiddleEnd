using HorreumStack.Domain.Entities;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public interface IAlmacenService
{
    Task<List<AlmacenDto>> GetAllAsync();
    Task<AlmacenDetailResponse> GetByIdAsync(Guid id, Guid userId);
    Task<AlmacenDto> GetByCodeAsync(string code);
    Task<AlmacenDto> CreateAsync(AlmacenDto model, Guid userId);
    Task<AlmacenDto> UpdateAsync(Guid id, AlmacenDto model);
    Task<bool> DeleteAsync(Guid id);
}