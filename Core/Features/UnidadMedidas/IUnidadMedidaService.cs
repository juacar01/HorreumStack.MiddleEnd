using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HorreumStack.MiddleEnd.Core.Features.UnidadMedidas;

public interface IUnidadMedidaService
{
    Task<List<UnidadMedidaResponse>> GetAllAsync();
    Task<List<UnidadMedidaResponse>> GetAllByProyectoIdAsync(Guid proyectoId);
    Task<UnidadMedidaPaginationResponse> GetPaginatedByProyectoIdAsync(Guid proyectoId, int pageIndex, int pageSize, string? search);
    Task<List<UnidadMedidaComboResponse>> GetAllComboByProyectoIdAsync(Guid proyectoId);
    Task<UnidadMedidaResponse> GetByIdAsync(Guid id);
    Task<UnidadMedidaResponse> CreateAsync(UnidadMedidaDto model);
    Task<UnidadMedidaResponse> UpdateAsync(Guid id, UnidadMedidaDto model);
    Task<bool> DeleteAsync(Guid id);
}
