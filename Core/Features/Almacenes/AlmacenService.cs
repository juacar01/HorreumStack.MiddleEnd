using System.Linq.Expressions;
using HorreumStack.Domain.Entities;
using AutoMapper;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.Domain.Entities.Common;
using HorreumStack.Domain.Enums;
using HorreumStack.MiddleEnd.Core.Constants;
using Microsoft.Identity.Client;
using HorreumStack.MiddleEnd.Core.Features.Users;
using HorreumStack.MiddleEnd.Core.Features.Ubicaciones;
using HorreumStack.MiddleEnd.Core.Features.Proyectos;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public class AlmacenService : IAlmacenService
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IProyectoService _proyectoService;

    public AlmacenService(IUnitOfWork unitOfWork, IMapper mapper, IProyectoService proyectoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _proyectoService = proyectoService;
    }

    public async Task<List<AlmacenDto>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<Almacen>().GetAllAsync();
        return _mapper.Map<List<AlmacenDto>>(list);
    }

    public async Task<AlmacenDetailResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var includesAlmacenes = new List<Expression<Func<Almacen, object>>>
        {
            au => au.Ubicaciones,
            au => au.Proyecto!
        };

        var almacen = await _unitOfWork.Repository<Almacen>().GetEntityAsync(
            a => a.Id == id && a.Status == AlmacenStatus.Active,
            includesAlmacenes,
            true
        );

        if (almacen == null)
        {
            return null!;
        }

        AlmacenDetailResponse response = _mapper.Map<AlmacenDetailResponse>(almacen);

        return response;
    }

    public async Task<AlmacenDto> GetByCodeAsync(string code)
    {
        throw new NotImplementedException();
    }

    public async Task<AlmacenDto> CreateAsync(AlmacenDto model, Guid userId)
    {
        bool isPrincipal = false;
        if (model.ProyectoId != Guid.Empty)
        {
            var proyecto = await _proyectoService.GetByIdAsync(model.ProyectoId, userId);
            isPrincipal = proyecto!.IsPrincipal;
        }

        var codigoGenerado = string.IsNullOrEmpty(model.Codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : model.Codigo;
        var almacen = _mapper.Map<Almacen>(model);
        almacen.Id = Guid.NewGuid();
        almacen.IsPrincipal = isPrincipal;
        almacen.Codigo = AppConstants.Almacenes.PrefixCodigo + codigoGenerado;
        almacen.CreatedAt = DateTime.UtcNow;
        almacen.CreatedBy = userId.ToString();
        almacen.LastModifiedAt = DateTime.UtcNow;
        almacen.LastModifiedBy = userId.ToString();

        var ubicacionAlmacen = new Ubicacion
        {
            Id = Guid.NewGuid(),
            AlmacenId = almacen.Id,
            Nombre = "Ubicacion principal de " + almacen.Nombre,
            IsPrincipal = true,
            Codigo = AppConstants.Ubicaciones.PrefixCodigo + codigoGenerado,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString(),
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        await _unitOfWork.Repository<Almacen>().AddAsync(almacen);
        await _unitOfWork.Repository<Ubicacion>().AddAsync(ubicacionAlmacen);
        await _unitOfWork.Complete();

        return _mapper.Map<AlmacenDto>(almacen);
    }

    public async Task<AlmacenDto> UpdateAsync(Guid id, AlmacenDto model)
    {
        var existing = await _unitOfWork.Repository<Almacen>().GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Almacén con ID {id} no encontrado");

        // Solo permitir actualizar ciertos campos
        existing.Nombre = model.Nombre;
        existing.Codigo = AppConstants.Almacenes.PrefixCodigo + model.Codigo.Replace(AppConstants.Almacenes.PrefixCodigo, "");
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