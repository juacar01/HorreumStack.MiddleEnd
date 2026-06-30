using System.Linq.Expressions;
using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.Domain.Enums;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.MiddleEnd.Core.Constants;
using HorreumStack.MiddleEnd.Core.Features.ItemTipos.Specification;

namespace HorreumStack.MiddleEnd.Core.Features.ItemTipos;

public class ItemTipoService : IItemTipoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ItemTipoService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ItemTipoResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<ItemTipo>().GetAsync(
            it => !it.IsDeleted
        );
        return _mapper.Map<List<ItemTipoResponse>>(list);
    }

    public async Task<List<ItemTipoResponse>> GetAllByProyectoIdAsync(Guid proyectoId)
    {
        var list = await _unitOfWork.Repository<ItemTipo>().GetAsync(
            it => it.ProyectoId == proyectoId && !it.IsDeleted
        );
        return _mapper.Map<List<ItemTipoResponse>>(list);
    }

    public async Task<ItemTipoPaginationResponse> GetPaginatedByProyectoIdAsync(Guid proyectoId, int pageIndex, int pageSize, string? search)
    {
        var spec = new ItemTiposSpecification(proyectoId, pageIndex, pageSize, search ?? "");
        var countSpec = new ItemTiposForCountSpecification(proyectoId, search ?? "");

        var totalItems = await _unitOfWork.Repository<ItemTipo>().CountAsync(countSpec);
        var list = await _unitOfWork.Repository<ItemTipo>().GetAllWithSpec(spec);

        var data = _mapper.Map<IReadOnlyList<ItemTipoResponse>>(list);

        return new ItemTipoPaginationResponse
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalItems,
            Data = data
        };
    }

    public async Task<List<ItemTipoComboResponse>> GetAllComboByProyectoIdAsync(Guid proyectoId)
    {
        var list = await _unitOfWork.Repository<ItemTipo>().GetAsync(
            it => it.ProyectoId == proyectoId && it.Status == GenericStatus.Active && !it.IsDeleted
        );
        return _mapper.Map<List<ItemTipoComboResponse>>(list);
    }

    public async Task<ItemTipoResponse> GetByIdAsync(Guid id)
    {
        var includes = new List<Expression<Func<ItemTipo, object>>>
        {
            it => it.Proyecto!
        };

        var item = await _unitOfWork.Repository<ItemTipo>().GetEntityAsync(
            it => it.Id == id && !it.IsDeleted,
            includes,
            true
        );

        return _mapper.Map<ItemTipoResponse>(item);
    }

    public async Task<ItemTipoResponse> CreateAsync(ItemTipoDto model)
    {
        var codigoGenerado = string.IsNullOrEmpty(model.Codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : model.Codigo;
        var item = _mapper.Map<ItemTipo>(model);
        item.Id = Guid.NewGuid();
        item.Codigo = AppConstants.ItemTipos.PrefixCodigo + codigoGenerado;
        item.CreatedAt = DateTime.UtcNow;
        item.CreatedBy = model.CreatedBy ?? "System";
        item.LastModifiedAt = DateTime.UtcNow;
        item.LastModifiedBy = model.CreatedBy ?? "System";

        await _unitOfWork.Repository<ItemTipo>().AddAsync(item);
        await _unitOfWork.Complete();

        return _mapper.Map<ItemTipoResponse>(item);
    }

    public async Task<ItemTipoResponse> UpdateAsync(Guid id, ItemTipoDto model)
    {
        var existing = await _unitOfWork.Repository<ItemTipo>().GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Tipo de Item con ID {id} no encontrado");

        existing.Nombre = model.Nombre;
        existing.Codigo = AppConstants.ItemTipos.PrefixCodigo + model.Codigo.Replace(AppConstants.ItemTipos.PrefixCodigo, "");
        existing.LastModifiedAt = DateTime.UtcNow;
        existing.LastModifiedBy = model.LastModifiedBy ?? "System";

        await _unitOfWork.Repository<ItemTipo>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return _mapper.Map<ItemTipoResponse>(existing);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _unitOfWork.Repository<ItemTipo>().GetByIdAsync(id);
        if (existing == null)
            return false;

        existing.IsDeleted = true;
        existing.DeletedAt = DateTime.UtcNow;
        existing.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<ItemTipo>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return true;
    }
}
