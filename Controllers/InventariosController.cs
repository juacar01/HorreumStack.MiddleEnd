using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HorreumStack.MiddleEnd.Core.Features.Inventarios;
using HorreumStack.MiddleEnd.Core.Features.Users;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace HorreumStack.MiddleEnd.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InventariosController : ControllerBase
{
    private readonly IInventarioService _inventarioService;
    private readonly IUserService _userService;

    public InventariosController(IInventarioService inventarioService, IUserService userService)
    {
        _inventarioService = inventarioService;
        _userService = userService;
    }

    [HttpGet("ubicacion/{ubicacionId}")]
    public async Task<IActionResult> GetInventarioByUbicacion(Guid ubicacionId, CancellationToken cancellationToken)
    {
        var list = await _inventarioService.GetInventarioByUbicacionAsync(ubicacionId, cancellationToken);
        return Ok(list);
    }

    [HttpGet("almacen/{almacenId}")]
    public async Task<IActionResult> GetInventarioByAlmacen(Guid almacenId, CancellationToken cancellationToken)
    {
        var list = await _inventarioService.GetInventarioByAlmacenAsync(almacenId, cancellationToken);
        return Ok(list);
    }

    [HttpGet("proyecto/{proyectoId}")]
    public async Task<IActionResult> GetInventarioByProyecto(Guid proyectoId, CancellationToken cancellationToken)
    {
        var list = await _inventarioService.GetInventarioByProyectoAsync(proyectoId, cancellationToken);
        return Ok(list);
    }

    [HttpGet("ubicacion/{ubicacionId}/movimientos")]
    public async Task<IActionResult> GetMovimientosByUbicacion(Guid ubicacionId, CancellationToken cancellationToken)
    {
        var list = await _inventarioService.GetMovimientosByUbicacionAsync(ubicacionId, cancellationToken);
        return Ok(list);
    }

    [HttpPost("ubicacion/{ubicacionId}/cargar")]
    public async Task<IActionResult> CargarStock(Guid ubicacionId, [FromBody] CargarInventarioDto dto, CancellationToken cancellationToken)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        Guid? userId = await _userService.GetUserIdByTokenAsync(token, cancellationToken);
        if (userId == null)
        {
            return Unauthorized();
        }

        if (dto.Cantidad == 0)
        {
            return BadRequest("La cantidad no puede ser cero.");
        }

        await _inventarioService.CargarStockAsync(ubicacionId, dto, userId.Value.ToString(), cancellationToken);
        return Ok(new { Message = "Stock cargado exitosamente." });
    }

    [HttpPost("ubicacion/{ubicacionId}/descargar")]
    public async Task<IActionResult> DescargarStock(Guid ubicacionId, [FromBody] CargarInventarioDto dto, CancellationToken cancellationToken)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        Guid? userId = await _userService.GetUserIdByTokenAsync(token, cancellationToken);
        if (userId == null)
        {
            return Unauthorized();
        }

        if (dto.Cantidad == 0)
        {
            return BadRequest("La cantidad no puede ser cero.");
        }

        await _inventarioService.DescargarStockAsync(ubicacionId, dto, userId.Value.ToString(), cancellationToken);
        return Ok(new { Message = "Stock descargado exitosamente." });
    }

    [HttpPost("ubicacion/{ubicacionId}/mover")]
    public async Task<IActionResult> MoverStock(Guid ubicacionId, [FromBody] MoverInventarioDto dto, CancellationToken cancellationToken)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        Guid? userId = await _userService.GetUserIdByTokenAsync(token, cancellationToken);
        if (userId == null)
        {
            return Unauthorized();
        }

        if (dto.Cantidad <= 0)
        {
            return BadRequest("La cantidad a trasladar debe ser mayor que cero.");
        }

        if (dto.UbicacionDestinoId == Guid.Empty || dto.UbicacionDestinoId == ubicacionId)
        {
            return BadRequest("Ubicación destino inválida.");
        }

        await _inventarioService.MoverStockAsync(ubicacionId, dto, userId.Value.ToString(), cancellationToken);
        return Ok(new { Message = "Stock trasladado exitosamente." });
    }
}
