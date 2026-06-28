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

    public async Task<AlmacenResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var includesAlmacenes = new List<Expression<Func<Almacen, object>>>
        {
            au => au.SubAlmacenes,
            au => au.Ubicaciones,
            au => au.AlmacenUsers
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

        var subalmacenes = almacen.SubAlmacenes.Where(sa => sa.Status == AlmacenStatus.Active).ToList();

        AlmacenResponse response = _mapper.Map<AlmacenResponse>(almacen);

        // Buscar el propietario del almacén a través de AlmacenUser
        var ownerRelation = await _unitOfWork.Repository<AlmacenUser>().GetEntityAsync(
            au => au.AlmacenId == id && au.Role == AlmacenUserRole.Owner,
            new List<Expression<Func<AlmacenUser, object>>> { au => au.User },
            true
        );

        // Si no se encuentra relación directa y tiene padre, buscar el propietario del padre
        if (ownerRelation == null && almacen.PadreAlmacenId.HasValue)
        {
            ownerRelation = await _unitOfWork.Repository<AlmacenUser>().GetEntityAsync(
                au => au.AlmacenId == almacen.PadreAlmacenId.Value && au.Role == AlmacenUserRole.Owner,
                new List<Expression<Func<AlmacenUser, object>>> { au => au.User },
                true
            );
        }

        Guid ownerId = Guid.Empty;
        string ownerName = string.Empty;
        bool isMine = false;

        if (ownerRelation != null)
        {
            UserDto userOwnerDto = _mapper.Map<UserDto>(ownerRelation.User);
            ownerId = userOwnerDto.Id;
            ownerName = userOwnerDto.Fullname;
            isMine = userOwnerDto.Id == userId;
        }

        response.IsMine = isMine;
        response.OwnerName = ownerName;
        response.OwnerId = ownerId;

        // Si tiene un almacén padre, obtener sus datos y asignarlos
        if (almacen.PadreAlmacenId.HasValue)
        {
            var padre = await _unitOfWork.Repository<Almacen>().GetEntityAsync(
                p => p.Id == almacen.PadreAlmacenId.Value && p.Status == AlmacenStatus.Active,
                null,
                true
            );
            if (padre != null)
            {
                var padreDto = _mapper.Map<AlmacenDto>(padre);
                padreDto.IsMine = isMine;
                padreDto.OwnerId = ownerId;
                padreDto.OwnerName = ownerName;
                response.Parent = padreDto;
            }
        }

        response.SubAlmacenes = subalmacenes.Select(sa =>
        {
            var dto = _mapper.Map<AlmacenDto>(sa);
            dto.IsMine = isMine;
            dto.OwnerName = ownerName;
            dto.OwnerId = ownerId;
            return dto;
        }).ToList();

        //obtener invitados
        //TODO: Implementar invitados, realizar el rpoceso de registro o login para aceptar 
        /*var invitados = await _unitOfWork.Repository<AlmacenUser>().GetAsync(
            au => au.AlmacenId == id && au.Status == AlmacenUserStatus.Active && au.UserId != userId,
            null,
            null,
            true
        );

        var invitadosDto = invitados.Select(inv =>
        {
            var dto = _mapper.Map<UserDto>(inv.User);
            return dto;
        }).ToList();

        response.Invitados = invitadosDto;
*/
        return response;
    }

    public async Task<AlmacenDto> GetByCodeAsync(string code)
    {
        throw new NotImplementedException();
    }

    //solo se obtienen los almacenes principales que no tiene padre
    public async Task<List<AlmacenDto>> GetListByUserIdAsync(Guid userId)
    {
        var includes = new List<Expression<Func<AlmacenUser, object>>>
        {
            au => au.Almacen,
            au => au.User
        };

        var userAlmacenes = await _unitOfWork.Repository<AlmacenUser>().GetAsync(
            au => au.UserId == userId && au.Status == AlmacenUserStatus.Active && au.Almacen.PadreAlmacenId == null,
            null,
            includes,
            true
        );

        var dtos = userAlmacenes.Select(au =>
        {
            var dto = _mapper.Map<AlmacenDto>(au.Almacen);
            dto.IsMine = au.Role == AlmacenUserRole.Owner;
            dto.OwnerName = au.User.Fullname;
            dto.OwnerId = au.User.Id;
            return dto;
        }).ToList();

        return dtos;
    }

    public async Task<AlmacenDto> CreateAsync(AlmacenDto model, Guid userId)
    {
        var codigoGenerado = string.IsNullOrEmpty(model.Codigo) ? Guid.NewGuid().ToString().Substring(0, 8) : model.Codigo;
        var almacen = _mapper.Map<Almacen>(model);
        almacen.Id = Guid.NewGuid();
        almacen.Codigo = AppConstants.Almacenes.PrefixCodigo + codigoGenerado;
        almacen.PadreAlmacenId = model.PadreAlmacenId == Guid.Empty || model.PadreAlmacenId == null ? null : model.PadreAlmacenId;
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

        var ubicacionAlmacen = new Ubicacion
        {
            Id = Guid.NewGuid(),
            AlmacenId = almacen.Id,
            Nombre = almacen.Nombre,
            IsPrincipal = true,
            Codigo = AppConstants.Ubicaciones.PrefixCodigo + codigoGenerado,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId.ToString(),
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        await _unitOfWork.Repository<Almacen>().AddAsync(almacen);
        await _unitOfWork.Repository<AlmacenUser>().AddAsync(userAlmacen);
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