using System.Linq.Expressions;
using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.Domain.Enums;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.MiddleEnd.Core.Constants;
using HorreumStack.MiddleEnd.Core.Features.UbicacionesTipos.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.UbicacionesTipos;

public class UbicacionTipoService : IUbicacionTipoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UbicacionTipoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<UbicacionTipoResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<UbicacionTipo>().GetAsync(
            ut => !ut.IsDeleted
        );
        return _mapper.Map<List<UbicacionTipoResponse>>(list);
    }

    public async Task<List<UbicacionTipoResponse>> GetAllByProyectoIdAsync(Guid proyectoId)
    {
        var list = await _unitOfWork.Repository<UbicacionTipo>().GetAsync(
            ut => ut.ProyectoId == proyectoId && !ut.IsDeleted
        );
        return _mapper.Map<List<UbicacionTipoResponse>>(list);
    }

    public async Task<UbicacionTipoPaginationResponse> GetPaginatedByProyectoIdAsync(Guid proyectoId, int pageIndex, int pageSize, string? search)
    {
        var spec = new UbicacionesTiposSpecification(proyectoId, pageIndex, pageSize, search ?? "");
        var countSpec = new UbicacionesTiposForCountSpecification(proyectoId, search ?? "");

        var totalItems = await _unitOfWork.Repository<UbicacionTipo>().CountAsync(countSpec);
        var list = await _unitOfWork.Repository<UbicacionTipo>().GetAllWithSpec(spec);

        var data = _mapper.Map<IReadOnlyList<UbicacionTipoResponse>>(list);

        return new UbicacionTipoPaginationResponse
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalItems,
            Data = data
        };
    }

    public async Task<UbicacionTipoResponse> GetByIdAsync(Guid id)
    {
        var includes = new List<Expression<Func<UbicacionTipo, object>>>
        {
            ut => ut.Proyecto!
        };

        var item = await _unitOfWork.Repository<UbicacionTipo>().GetEntityAsync(
            ut => ut.Id == id && !ut.IsDeleted,
            includes,
            true
        );

        return _mapper.Map<UbicacionTipoResponse>(item);
    }

    public async Task<UbicacionTipoResponse> CreateAsync(UbicacionTipoDto model)
    {
        var codigoGenerado = string.IsNullOrEmpty(model.Codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : model.Codigo;
        var item = _mapper.Map<UbicacionTipo>(model);
        item.Id = Guid.NewGuid();
        item.Codigo = AppConstants.UbicacionesTipos.PrefixCodigo + codigoGenerado;
        item.CreatedAt = DateTime.UtcNow;
        item.CreatedBy = model.CreatedBy ?? "System";
        item.LastModifiedAt = DateTime.UtcNow;
        item.LastModifiedBy = model.CreatedBy ?? "System";

        await _unitOfWork.Repository<UbicacionTipo>().AddAsync(item);
        await _unitOfWork.Complete();

        return _mapper.Map<UbicacionTipoResponse>(item);
    }

    public async Task<UbicacionTipoResponse> UpdateAsync(Guid id, UbicacionTipoDto model)
    {
        var existing = await _unitOfWork.Repository<UbicacionTipo>().GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Tipo de Ubicación con ID {id} no encontrado");

        existing.Nombre = model.Nombre;
        existing.Codigo = AppConstants.UbicacionesTipos.PrefixCodigo + model.Codigo.Replace(AppConstants.UbicacionesTipos.PrefixCodigo, "");
        existing.LastModifiedAt = DateTime.UtcNow;
        existing.LastModifiedBy = model.LastModifiedBy ?? "System";

        await _unitOfWork.Repository<UbicacionTipo>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return _mapper.Map<UbicacionTipoResponse>(existing);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _unitOfWork.Repository<UbicacionTipo>().GetByIdAsync(id);
        if (existing == null)
            return false;

        existing.IsDeleted = true;
        existing.DeletedAt = DateTime.UtcNow;
        existing.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<UbicacionTipo>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return true;
    }

    public async Task<List<UbicacionTipoComboResponse>> GetAllComboByProyectoIdAsync(Guid proyectoId)
    {
        var list = await _unitOfWork.Repository<UbicacionTipo>().GetAsync(
            ut => ut.ProyectoId == proyectoId && ut.Status == GenericStatus.Active
        );
        return _mapper.Map<List<UbicacionTipoComboResponse>>(list);
    }
}
