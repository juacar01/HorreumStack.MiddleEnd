using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Repositories;

namespace HorreumStack.MiddleEnd.Core.Features.Inventarios;

public class InventarioService : IInventarioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventarioService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<InventarioResponse>> GetInventarioByUbicacionAsync(Guid ubicacionId, CancellationToken cancellationToken)
    {
        var includes = new List<Expression<Func<Inventario, object>>>
        {
            i => i.Item,
            i => i.Item.ItemTipo!
        };
        var list = await _unitOfWork.Repository<Inventario>().GetAsync(
            i => i.UbicacionId == ubicacionId && !i.IsDeleted,
            orderBy: q => q.OrderBy(i => i.Item.Nombre),
            includes: includes
        );
        return _mapper.Map<List<InventarioResponse>>(list.ToList());
    }

    public async Task<List<InventarioMovimientoResponse>> GetMovimientosByUbicacionAsync(Guid ubicacionId, CancellationToken cancellationToken)
    {
        var includes = new List<Expression<Func<InventarioMovimiento, object>>>
        {
            im => im.Item,
            im => im.Item.ItemTipo!,
            im => im.UbicacionOrigen!,
            im => im.UbicacionDestino!
        };
        var list = await _unitOfWork.Repository<InventarioMovimiento>().GetAsync(
            im => (im.UbicacionOrigenId == ubicacionId || im.UbicacionDestinoId == ubicacionId) && !im.IsDeleted,
            orderBy: q => q.OrderByDescending(im => im.CreatedAt),
            includes: includes
        );
        return _mapper.Map<List<InventarioMovimientoResponse>>(list.ToList());
    }

    public async Task CargarStockAsync(Guid ubicacionId, CargarInventarioDto dto, string userEmail, CancellationToken cancellationToken)
    {
        var inventarioList = await _unitOfWork.Repository<Inventario>().GetAsync(
            i => i.UbicacionId == ubicacionId && i.ItemId == dto.ItemId && !i.IsDeleted
        );
        var inventario = inventarioList.FirstOrDefault();

        if (inventario == null)
        {
            inventario = new Inventario
            {
                ItemId = dto.ItemId,
                UbicacionId = ubicacionId,
                Cantidad = dto.Cantidad,
                CreatedBy = userEmail
            };
            await _unitOfWork.Repository<Inventario>().AddAsync(inventario);
        }
        else
        {
            inventario.Cantidad += dto.Cantidad;
            inventario.LastModifiedBy = userEmail;
            inventario.LastModifiedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Inventario>().UpdateAsync(inventario);
        }

        var movimiento = new InventarioMovimiento
        {
            ItemId = dto.ItemId,
            UbicacionDestinoId = ubicacionId,
            Cantidad = dto.Cantidad,
            TipoMovimiento = "CARGA",
            Observaciones = dto.Observaciones,
            CreatedBy = userEmail
        };
        await _unitOfWork.Repository<InventarioMovimiento>().AddAsync(movimiento);

        await _unitOfWork.Complete();
    }

    public async Task DescargarStockAsync(Guid ubicacionId, CargarInventarioDto dto, string userEmail, CancellationToken cancellationToken)
    {
        var inventarioList = await _unitOfWork.Repository<Inventario>().GetAsync(
            i => i.UbicacionId == ubicacionId && i.ItemId == dto.ItemId && !i.IsDeleted
        );
        var inventario = inventarioList.FirstOrDefault();

        if (inventario == null)
        {
            inventario = new Inventario
            {
                ItemId = dto.ItemId,
                UbicacionId = ubicacionId,
                Cantidad = -dto.Cantidad,
                CreatedBy = userEmail
            };
            await _unitOfWork.Repository<Inventario>().AddAsync(inventario);
        }
        else
        {
            inventario.Cantidad -= dto.Cantidad;
            inventario.LastModifiedBy = userEmail;
            inventario.LastModifiedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Inventario>().UpdateAsync(inventario);
        }

        var movimiento = new InventarioMovimiento
        {
            ItemId = dto.ItemId,
            UbicacionOrigenId = ubicacionId,
            Cantidad = dto.Cantidad,
            TipoMovimiento = "DESCARGA",
            Observaciones = dto.Observaciones,
            CreatedBy = userEmail
        };
        await _unitOfWork.Repository<InventarioMovimiento>().AddAsync(movimiento);

        await _unitOfWork.Complete();
    }

    public async Task MoverStockAsync(Guid ubicacionOrigenId, MoverInventarioDto dto, string userEmail, CancellationToken cancellationToken)
    {
        var inventarioOrigenList = await _unitOfWork.Repository<Inventario>().GetAsync(
            i => i.UbicacionId == ubicacionOrigenId && i.ItemId == dto.ItemId && !i.IsDeleted
        );
        var inventarioOrigen = inventarioOrigenList.FirstOrDefault();
        if (inventarioOrigen == null)
        {
            inventarioOrigen = new Inventario
            {
                ItemId = dto.ItemId,
                UbicacionId = ubicacionOrigenId,
                Cantidad = -dto.Cantidad,
                CreatedBy = userEmail
            };
            await _unitOfWork.Repository<Inventario>().AddAsync(inventarioOrigen);
        }
        else
        {
            inventarioOrigen.Cantidad -= dto.Cantidad;
            inventarioOrigen.LastModifiedBy = userEmail;
            inventarioOrigen.LastModifiedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Inventario>().UpdateAsync(inventarioOrigen);
        }

        var inventarioDestinoList = await _unitOfWork.Repository<Inventario>().GetAsync(
            i => i.UbicacionId == dto.UbicacionDestinoId && i.ItemId == dto.ItemId && !i.IsDeleted
        );
        var inventarioDestino = inventarioDestinoList.FirstOrDefault();
        if (inventarioDestino == null)
        {
            inventarioDestino = new Inventario
            {
                ItemId = dto.ItemId,
                UbicacionId = dto.UbicacionDestinoId,
                Cantidad = dto.Cantidad,
                CreatedBy = userEmail
            };
            await _unitOfWork.Repository<Inventario>().AddAsync(inventarioDestino);
        }
        else
        {
            inventarioDestino.Cantidad += dto.Cantidad;
            inventarioDestino.LastModifiedBy = userEmail;
            inventarioDestino.LastModifiedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Inventario>().UpdateAsync(inventarioDestino);
        }

        var movimiento = new InventarioMovimiento
        {
            ItemId = dto.ItemId,
            UbicacionOrigenId = ubicacionOrigenId,
            UbicacionDestinoId = dto.UbicacionDestinoId,
            Cantidad = dto.Cantidad,
            TipoMovimiento = "MOVIMIENTO",
            Observaciones = dto.Observaciones,
            CreatedBy = userEmail
        };
        await _unitOfWork.Repository<InventarioMovimiento>().AddAsync(movimiento);

        await _unitOfWork.Complete();
    }

    public async Task<List<AlmacenItemStockResponse>> GetInventarioByAlmacenAsync(Guid almacenId, CancellationToken cancellationToken)
    {
        var locations = await _unitOfWork.Repository<Ubicacion>().GetAsync(u => u.AlmacenId == almacenId && !u.IsDeleted);
        var locationIds = locations.Select(l => l.Id).ToList();

        if (!locationIds.Any())
        {
            return new List<AlmacenItemStockResponse>();
        }

        var includes = new List<Expression<Func<Inventario, object>>>
        {
            i => i.Item!,
            i => i.Item!.ItemTipo!,
            i => i.Item!.UnidadMedida!
        };

        var records = await _unitOfWork.Repository<Inventario>().GetAsync(
            i => locationIds.Contains(i.UbicacionId) && i.Cantidad != 0,
            orderBy: null,
            includes: includes
        );

        var grouped = records
            .GroupBy(i => i.ItemId)
            .Select(g => new AlmacenItemStockResponse
            {
                ItemId = g.Key,
                ItemNombre = g.First().Item?.Nombre ?? "Item desconocido",
                ItemCodigo = g.First().Item?.Codigo ?? "",
                ItemTipoNombre = g.First().Item?.ItemTipo?.Nombre ?? "--",
                UnidadMedidaNombre = g.First().Item?.UnidadMedida?.Nombre ?? "--",
                CantidadTotal = g.Sum(i => i.Cantidad),
                Imagen = g.First().Item?.Imagen ?? ""
            })
            .ToList();

        return grouped;
    }

    public async Task<List<AlmacenItemStockResponse>> GetInventarioByProyectoAsync(Guid proyectoId, CancellationToken cancellationToken)
    {
        var warehouses = await _unitOfWork.Repository<Almacen>().GetAsync(a => a.ProyectoId == proyectoId && !a.IsDeleted);
        var warehouseIds = warehouses.Select(w => w.Id).ToList();

        if (!warehouseIds.Any())
        {
            return new List<AlmacenItemStockResponse>();
        }

        var locations = await _unitOfWork.Repository<Ubicacion>().GetAsync(u => warehouseIds.Contains(u.AlmacenId) && !u.IsDeleted);
        var locationIds = locations.Select(l => l.Id).ToList();

        if (!locationIds.Any())
        {
            return new List<AlmacenItemStockResponse>();
        }

        var includes = new List<Expression<Func<Inventario, object>>>
        {
            i => i.Item!,
            i => i.Item!.ItemTipo!,
            i => i.Item!.UnidadMedida!
        };

        var records = await _unitOfWork.Repository<Inventario>().GetAsync(
            i => locationIds.Contains(i.UbicacionId) && i.Cantidad != 0,
            orderBy: null,
            includes: includes
        );

        var grouped = records
            .GroupBy(i => i.ItemId)
            .Select(g => new AlmacenItemStockResponse
            {
                ItemId = g.Key,
                ItemNombre = g.First().Item?.Nombre ?? "Item desconocido",
                ItemCodigo = g.First().Item?.Codigo ?? "",
                ItemTipoNombre = g.First().Item?.ItemTipo?.Nombre ?? "--",
                UnidadMedidaNombre = g.First().Item?.UnidadMedida?.Nombre ?? "--",
                CantidadTotal = g.Sum(i => i.Cantidad),
                Imagen = g.First().Item?.Imagen ?? ""
            })
            .ToList();

        return grouped;
    }
}
