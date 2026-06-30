using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HorreumStack.MiddleEnd.Core.Features.Ubicaciones;
using HorreumStack.MiddleEnd.Core.Features.Users;

namespace HorreumStack.MiddleEnd.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UbicacionesController : ControllerBase
{
    private readonly IUbicacionService _ubicacionService;
    private readonly IUserService _userService;

    public UbicacionesController(IUbicacionService ubicacionService, IUserService userService)
    {
        _ubicacionService = ubicacionService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ubicaciones = await _ubicacionService.GetAllAsync();
        return Ok(ubicaciones);
    }

    [HttpGet("almacen/{almacenId}")]
    public async Task<IActionResult> GetAllByAlmacenId(Guid almacenId)
    {
        var ubicaciones = await _ubicacionService.GetAllByAlmacenIdAsync(almacenId);
        return Ok(ubicaciones);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ubicacion = await _ubicacionService.GetByIdAsync(id);
        if (ubicacion == null) 
            return NotFound("Ubicación no encontrada.");

        return Ok(ubicacion);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] UbicacionDto model)
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(model.Nombre))
        {
            return BadRequest("El nombre es obligatorio.");
        }

        model.CreatedBy = userId.Value.ToString();

        var ubicacion = await _ubicacionService.CreateAsync(model);
        if (ubicacion.Id == Guid.Empty) 
            return BadRequest("Error al crear la ubicación.");

        var ubicacionResult = await _ubicacionService.GetByIdAsync(ubicacion.Id);
        if (ubicacionResult == null) 
            return BadRequest("Error al obtener la ubicación.");

        return CreatedAtAction(nameof(GetById), new { id = ubicacion.Id }, ubicacionResult);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UbicacionDto model)
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(model.Nombre))
        {
            return BadRequest("El nombre es obligatorio.");
        }

        model.LastModifiedBy = userId.Value.ToString();

        try
        {
            var updated = await _ubicacionService.UpdateAsync(id, model);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Ubicación no encontrada.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _ubicacionService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound("Ubicación no encontrada.");
        }

        return Ok(new { Message = "Ubicación eliminada correctamente." });
    }
}
