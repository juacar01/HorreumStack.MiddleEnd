using HorreumStack.Domain.Entities;

namespace HorreumStack.MiddleEnd.Core.Features.Proyectos;

public interface IProyectoService
{
    Task<List<ProyectoDto>> GetAllAsync();
    Task<ProyectoDetailResponse> GetByIdAsync(Guid id, Guid userId);
    Task<ProyectoDto> GetByCodeAsync(string code);
    Task<List<ProyectoResponse>> GetListByUserIdAsync(Guid userId);
    Task<ProyectoDto> CreateAsync(ProyectoDto model, Guid userId);
    Task<ProyectoDto> UpdateAsync(Guid id, ProyectoDto model);
    Task<bool> DeleteAsync(Guid id);
}