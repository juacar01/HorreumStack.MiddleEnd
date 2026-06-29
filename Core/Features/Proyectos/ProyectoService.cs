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
using HorreumStack.MiddleEnd.Core.Features.Almacenes;

namespace HorreumStack.MiddleEnd.Core.Features.Proyectos;

public class ProyectoService : IProyectoService
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public ProyectoService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userService = userService;
    }

    public async Task<List<ProyectoDto>> GetAllAsync()
    {
        var list = await _unitOfWork.Repository<Proyecto>().GetAllAsync();
        return _mapper.Map<List<ProyectoDto>>(list);
    }

    public async Task<ProyectoDetailResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var includesProyectos = new List<Expression<Func<Proyecto, object>>>
        {
            au => au.Almacenes,
            au => au.Owner
        };

        var proyecto = await _unitOfWork.Repository<Proyecto>().GetEntityAsync(
            a => a.Id == id && a.Status == ProyectoStatus.Active,
            includesProyectos,
            true
        );

        if (proyecto == null)
        {
            return null!;
        }

        var response = _mapper.Map<ProyectoDetailResponse>(proyecto);
        response.IsMine = proyecto.OwnerId == userId;
        return response;
    }

    public async Task<ProyectoDto> GetByCodeAsync(string code)
    {
        throw new NotImplementedException();
    }

    //solo se obtienen los almacenes principales que no tiene padre
    public async Task<List<ProyectoResponse>> GetListByUserIdAsync(Guid userId)
    {
        var includesProyecto = new List<Expression<Func<Proyecto, object>>>
        {
            au => au.Owner
        };

        var proyectos = await _unitOfWork.Repository<Proyecto>().GetAsync(
             a => a.Status == ProyectoStatus.Active && (a.OwnerId == userId || a.ProyectoUsers.Any(pu => pu.UserId == userId && pu.Status == ProyectoUserStatus.Active)),
             null,
             includesProyecto,
             true
         );

        var mapped = _mapper.Map<List<ProyectoResponse>>(proyectos);
        mapped.ForEach(p => p.IsMine = p.OwnerId == userId);
        return mapped;
    }

    public async Task<ProyectoDto> CreateAsync(ProyectoDto model, Guid userId)
    {
        UserVm userDto = await _userService.GetUserByIdAsync(userId);

        if (userDto == null)
        {
            throw new Exception("Usuario no encontrado");
        }
        bool IsPrincipal = userDto.Proyectos.Count == 0;

        var codigoGenerado = string.IsNullOrEmpty(model.Codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : model.Codigo;
        var proyecto = _mapper.Map<Proyecto>(model);
        proyecto.Id = Guid.NewGuid();
        proyecto.Codigo = AppConstants.Proyectos.PrefixCodigo + codigoGenerado;
        proyecto.OwnerId = userId;
        proyecto.IsPrincipal = IsPrincipal;
        proyecto.CreatedAt = DateTime.UtcNow;
        proyecto.CreatedBy = userId.ToString();
        proyecto.LastModifiedAt = DateTime.UtcNow;
        proyecto.LastModifiedBy = userId.ToString();

        var userProyecto = new ProyectoUser
        {
            UserId = userId,
            ProyectoId = proyecto.Id,
            Role = ProyectoUserRole.Owner
        };

        Almacen almacen = new Almacen
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyecto.Id,
            Nombre = "Almacén principal",
            Descripcion = "Almacén principal de " + proyecto.Nombre,
            Codigo = AppConstants.Almacenes.PrefixCodigo + codigoGenerado,
            IsPrincipal = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString(),
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        UbicacionTipo ubicacionTipo = new UbicacionTipo
        {
            Id = Guid.NewGuid(),
            Nombre = "Almacén",
            Codigo = AppConstants.UbicacionesTipos.PrefixCodigo + codigoGenerado,
            ProyectoId = proyecto.Id,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString(),
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        Ubicacion ubicacion = new Ubicacion
        {
            Id = Guid.NewGuid(),
            Nombre = "Ubicación principal",
            IsPrincipal = true,
            AlmacenId = almacen.Id,
            ubicacionTipoId = ubicacionTipo.Id,
            Codigo = AppConstants.Ubicaciones.PrefixCodigo + codigoGenerado,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString(),
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        await _unitOfWork.Repository<Proyecto>().AddAsync(proyecto);
        await _unitOfWork.Repository<ProyectoUser>().AddAsync(userProyecto);
        await _unitOfWork.Repository<Almacen>().AddAsync(almacen);
        await _unitOfWork.Repository<UbicacionTipo>().AddAsync(ubicacionTipo);
        await _unitOfWork.Repository<Ubicacion>().AddAsync(ubicacion);
        await _unitOfWork.Complete();

        return _mapper.Map<ProyectoDto>(proyecto);
    }

    public async Task<ProyectoDto> UpdateAsync(Guid id, ProyectoDto model)
    {
        var existing = await _unitOfWork.Repository<Proyecto>().GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Proyecto con ID {id} no encontrado");

        // Solo permitir actualizar ciertos campos
        existing.Nombre = model.Nombre;
        existing.Codigo = AppConstants.Proyectos.PrefixCodigo + model.Codigo.Replace(AppConstants.Proyectos.PrefixCodigo, "");
        existing.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Proyecto>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return _mapper.Map<ProyectoDto>(existing);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _unitOfWork.Repository<Proyecto>().GetByIdAsync(id);
        if (existing == null)
            return false;

        // Desactivar en lugar de borrar (recomendado para FKs)
        existing.Status = ProyectoStatus.Deleted;
        existing.IsDeleted = true;
        existing.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Proyecto>().UpdateAsync(existing);
        await _unitOfWork.Complete();

        return true;
    }
}