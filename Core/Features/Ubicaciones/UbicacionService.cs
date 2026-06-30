using System.Linq.Expressions;
using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.Domain.Enums;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.MiddleEnd.Core.Constants;

namespace HorreumStack.MiddleEnd.Core.Features.Ubicaciones;

public class UbicacionService : IUbicacionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UbicacionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<UbicacionResponse>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<Ubicacion>().GetAsync(
            u => u.Status == UbicacionStatus.Active && !u.IsDeleted
        );
        return _mapper.Map<List<UbicacionResponse>>(list);
    }

    public async Task<List<UbicacionResponse>> GetAllByAlmacenIdAsync(Guid almacenId)
    {
        var list = await _unitOfWork.Repository<Ubicacion>().GetAsync(
            u => u.AlmacenId == almacenId && u.Status == UbicacionStatus.Active && !u.IsDeleted
        );
        return _mapper.Map<List<UbicacionResponse>>(list);
    }

    public async Task<UbicacionDetailResponse> GetByIdAsync(Guid id)
    {
        var includes = new List<Expression<Func<Ubicacion, object>>>
        {
            u => u.Almacen!.Proyecto!,
            u => u.UbicacionPadre!
        };

        var ubicacion = await _unitOfWork.Repository<Ubicacion>().GetEntityAsync(
            u => u.Id == id && u.Status == UbicacionStatus.Active && !u.IsDeleted,
            includes,
            true
        );

        return _mapper.Map<UbicacionDetailResponse>(ubicacion);
    }

    public async Task<UbicacionResponse> CreateAsync(UbicacionDto model)
    {
        var codigoGenerado = string.IsNullOrEmpty(model.Codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : model.Codigo;
        var ubicacion = _mapper.Map<Ubicacion>(model);
        ubicacion.Id = Guid.NewGuid();
        ubicacion.Codigo = AppConstants.Ubicaciones.PrefixCodigo + codigoGenerado;
        ubicacion.CreatedAt = DateTime.UtcNow;
        ubicacion.CreatedBy = model.CreatedBy ?? "System";
        ubicacion.LastModifiedAt = DateTime.UtcNow;
        ubicacion.LastModifiedBy = model.CreatedBy ?? "System";

        await _unitOfWork.Repository<Ubicacion>().AddAsync(ubicacion);
        await _unitOfWork.Complete();

        return _mapper.Map<UbicacionResponse>(ubicacion);
    }

    public async Task<UbicacionResponse> UpdateAsync(Guid id, UbicacionDto model)
    {
        var existing = await _unitOfWork.Repository<Ubicacion>().GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Ubicación con ID {id} no encontrada");

        existing.Nombre = model.Nombre;
        existing.Codigo = AppConstants.Ubicaciones.PrefixCodigo + model.Codigo.Replace(AppConstants.Ubicaciones.PrefixCodigo, "");
        existing.ubicacionTipoId = model.ubicacionTipoId;
        existing.Status = model.Status;
        existing.LastModifiedAt = DateTime.UtcNow;
        existing.LastModifiedBy = model.LastModifiedBy ?? "System";

        await _unitOfWork.Repository<Ubicacion>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return _mapper.Map<UbicacionResponse>(existing);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _unitOfWork.Repository<Ubicacion>().GetByIdAsync(id);
        if (existing == null)
            return false;

        existing.Status = UbicacionStatus.Deleted;
        existing.IsDeleted = true;
        existing.DeletedAt = DateTime.UtcNow;
        existing.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Ubicacion>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return true;
    }
}