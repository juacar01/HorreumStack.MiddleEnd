using System.Linq.Expressions;
using HorreumStack.Domain.Entities;
using AutoMapper;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.Domain.Entities.Common;
using HorreumStack.Domain.Enums;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public class AlmacenService : IAlmacenService
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AlmacenService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<List<AlmacenDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<AlmacenDto> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<AlmacenDto> GetByCodeAsync(string code)
    {
        throw new NotImplementedException();
    }

    public async Task<List<AlmacenDto>> GetListByUserIdAsync(Guid userId)
    {
        var includes = new List<Expression<Func<AlmacenUser, object>>>
        {
            au => au.Almacen
        };

        var userAlmacenes = await _unitOfWork.Repository<AlmacenUser>().GetAsync(
            au => au.UserId == userId,
            null,
            includes,
            true
        );

        var almacenes = userAlmacenes.Select(au => au.Almacen).ToList();
        return _mapper.Map<List<AlmacenDto>>(almacenes);
    }

    public async Task<AlmacenDto> CreateAsync(AlmacenDto model, Guid userId)
    {
        var almacen = _mapper.Map<Almacen>(model);
        almacen.Id = Guid.NewGuid();
        almacen.CreatedAt = DateTime.UtcNow;
        almacen.CreatedBy = userId.ToString();
        almacen.LastModifiedAt = DateTime.UtcNow;
        almacen.LastModifiedBy = userId.ToString();

        var userAlmacen = new AlmacenUser
        {
            UserId = userId,
            AlmacenId = almacen.Id,
            Role = AlmacenUserRole.Owner
        };

        await _unitOfWork.Repository<Almacen>().AddAsync(almacen);
        await _unitOfWork.Repository<AlmacenUser>().AddAsync(userAlmacen);
        await _unitOfWork. Complete();

        return _mapper.Map<AlmacenDto>(almacen);
    }

    public async Task<AlmacenDto> UpdateAsync(Guid id, AlmacenDto model)
    {
        var existing = await _unitOfWork.Repository<Almacen>().GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Almacén con ID {id} no encontrado");

        // Solo permitir actualizar ciertos campos
        existing.Nombre = model.Nombre;
        existing.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Almacen>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return _mapper.Map<AlmacenDto>(existing);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _unitOfWork.Repository<Almacen>().GetByIdAsync(id);
        if (existing == null)
            return false;

        // Desactivar en lugar de borrar (recomendado para FKs)
        existing.Status = AlmacenStatus.Deleted;
        existing.IsDeleted = true;
        existing.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Almacen>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return true;
    }
}