using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HorreumStack.MiddleEnd.Core.Features.UbicacionesTipos;
using HorreumStack.MiddleEnd.Core.Features.Users;

namespace HorreumStack.MiddleEnd.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UbicacionesTiposController : ControllerBase
{
    private readonly IUbicacionTipoService _ubicacionTipoService;
    private readonly IUserService _userService;

    public UbicacionesTiposController(IUbicacionTipoService ubicacionTipoService, IUserService userService)
    {
        _ubicacionTipoService = ubicacionTipoService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _ubicacionTipoService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("proyecto/{proyectoId}")]
    public async Task<IActionResult> GetAllByProyectoId(
        Guid proyectoId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _ubicacionTipoService.GetPaginatedByProyectoIdAsync(proyectoId, pageIndex, pageSize, search);
        return Ok(result);
    }

    [HttpGet("proyecto/{proyectoId}/combo")]
    public async Task<IActionResult> GetAllComboByProyectoId(Guid proyectoId)
    {
        var result = await _ubicacionTipoService.GetAllComboByProyectoIdAsync(proyectoId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _ubicacionTipoService.GetByIdAsync(id);
        if (result == null)
            return NotFound("Tipo de Ubicación no encontrado.");

        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] UbicacionTipoDto model)
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

        var result = await _ubicacionTipoService.CreateAsync(model);
        if (result.Id == Guid.Empty)
            return BadRequest("Error al crear el tipo de ubicación.");

        var finalResult = await _ubicacionTipoService.GetByIdAsync(result.Id);
        if (finalResult == null)
            return BadRequest("Error al obtener el tipo de ubicación.");

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, finalResult);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UbicacionTipoDto model)
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
            var updated = await _ubicacionTipoService.UpdateAsync(id, model);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Tipo de Ubicación no encontrado.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _ubicacionTipoService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound("Tipo de Ubicación no encontrado.");
        }

        return Ok(new { Message = "Tipo de ubicación eliminado correctamente." });
    }
}
