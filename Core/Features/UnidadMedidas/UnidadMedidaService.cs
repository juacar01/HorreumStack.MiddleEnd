using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.Domain.Enums;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.MiddleEnd.Core.Constants;
using HorreumStack.MiddleEnd.Core.Features.UnidadMedidas.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.UnidadMedidas;

public class UnidadMedidaService : IUnidadMedidaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UnidadMedidaService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<UnidadMedidaResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<UnidadMedida>().GetAsync(
            um => !um.IsDeleted
        );
        return _mapper.Map<List<UnidadMedidaResponse>>(list);
    }

    public async Task<List<UnidadMedidaResponse>> GetAllByProyectoIdAsync(Guid proyectoId)
    {
        var list = await _unitOfWork.Repository<UnidadMedida>().GetAsync(
            um => um.ProyectoId == proyectoId && !um.IsDeleted
        );
        return _mapper.Map<List<UnidadMedidaResponse>>(list);
    }

    public async Task<UnidadMedidaPaginationResponse> GetPaginatedByProyectoIdAsync(Guid proyectoId, int pageIndex, int pageSize, string? search)
    {
        var spec = new UnidadMedidasSpecification(proyectoId, pageIndex, pageSize, search ?? "");
        var countSpec = new UnidadMedidasForCountSpecification(proyectoId, search ?? "");

        var totalItems = await _unitOfWork.Repository<UnidadMedida>().CountAsync(countSpec);
        var list = await _unitOfWork.Repository<UnidadMedida>().GetAllWithSpec(spec);

        var data = _mapper.Map<IReadOnlyList<UnidadMedidaResponse>>(list);

        return new UnidadMedidaPaginationResponse
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalItems,
            Data = data
        };
    }

    public async Task<List<UnidadMedidaComboResponse>> GetAllComboByProyectoIdAsync(Guid proyectoId)
    {
        var list = await _unitOfWork.Repository<UnidadMedida>().GetAsync(
            um => um.ProyectoId == proyectoId && um.Status == GenericStatus.Active && !um.IsDeleted
        );
        return _mapper.Map<List<UnidadMedidaComboResponse>>(list);
    }

    public async Task<UnidadMedidaResponse> GetByIdAsync(Guid id)
    {
        var includes = new List<Expression<Func<UnidadMedida, object>>>
        {
            um => um.Proyecto!
        };

        var item = await _unitOfWork.Repository<UnidadMedida>().GetEntityAsync(
            um => um.Id == id && !um.IsDeleted,
            includes,
            true
        );

        return _mapper.Map<UnidadMedidaResponse>(item);
    }

    public async Task<UnidadMedidaResponse> CreateAsync(UnidadMedidaDto model)
    {
        var codigoGenerado = string.IsNullOrEmpty(model.Codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : model.Codigo;
        var item = _mapper.Map<UnidadMedida>(model);
        item.Id = Guid.NewGuid();
        item.Codigo = AppConstants.UnidadMedidas.PrefixCodigo + codigoGenerado;
        item.CreatedAt = DateTime.UtcNow;
        item.CreatedBy = model.CreatedBy ?? "System";
        item.LastModifiedAt = DateTime.UtcNow;
        item.LastModifiedBy = model.CreatedBy ?? "System";

        await _unitOfWork.Repository<UnidadMedida>().AddAsync(item);
        await _unitOfWork.Complete();

        return _mapper.Map<UnidadMedidaResponse>(item);
    }

    public async Task<UnidadMedidaResponse> UpdateAsync(Guid id, UnidadMedidaDto model)
    {
        var existing = await _unitOfWork.Repository<UnidadMedida>().GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Unidad de Medida con ID {id} no encontrada");

        existing.Nombre = model.Nombre;
        existing.Codigo = AppConstants.UnidadMedidas.PrefixCodigo + model.Codigo.Replace(AppConstants.UnidadMedidas.PrefixCodigo, "");
        existing.LastModifiedAt = DateTime.UtcNow;
        existing.LastModifiedBy = model.LastModifiedBy ?? "System";

        await _unitOfWork.Repository<UnidadMedida>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return _mapper.Map<UnidadMedidaResponse>(existing);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _unitOfWork.Repository<UnidadMedida>().GetByIdAsync(id);
        if (existing == null)
            return false;

        existing.IsDeleted = true;
        existing.DeletedAt = DateTime.UtcNow;
        existing.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<UnidadMedida>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return true;
    }
}
