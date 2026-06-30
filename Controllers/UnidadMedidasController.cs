using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HorreumStack.MiddleEnd.Core.Features.UnidadMedidas;
using HorreumStack.MiddleEnd.Core.Features.Users;

namespace HorreumStack.MiddleEnd.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UnidadMedidasController : ControllerBase
{
    private readonly IUnidadMedidaService _unidadMedidaService;
    private readonly IUserService _userService;

    public UnidadMedidasController(IUnidadMedidaService unidadMedidaService, IUserService userService)
    {
        _unidadMedidaService = unidadMedidaService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _unidadMedidaService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("proyecto/{proyectoId}")]
    public async Task<IActionResult> GetAllByProyectoId(
        Guid proyectoId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _unidadMedidaService.GetPaginatedByProyectoIdAsync(proyectoId, pageIndex, pageSize, search);
        return Ok(result);
    }

    [HttpGet("proyecto/{proyectoId}/combo")]
    public async Task<IActionResult> GetAllComboByProyectoId(Guid proyectoId)
    {
        var result = await _unidadMedidaService.GetAllComboByProyectoIdAsync(proyectoId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _unidadMedidaService.GetByIdAsync(id);
        if (result == null)
            return NotFound("Unidad de medida no encontrada.");

        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] UnidadMedidaDto model)
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

        var result = await _unidadMedidaService.CreateAsync(model);
        if (result.Id == Guid.Empty)
            return BadRequest("Error al crear la unidad de medida.");

        var finalResult = await _unidadMedidaService.GetByIdAsync(result.Id);
        if (finalResult == null)
            return BadRequest("Error al obtener la unidad de medida.");

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, finalResult);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UnidadMedidaDto model)
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
            var updated = await _unidadMedidaService.UpdateAsync(id, model);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Unidad de medida no encontrada.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _unidadMedidaService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound("Unidad de medida no encontrada.");
        }

        return Ok(new { Message = "Unidad de medida eliminada correctamente." });
    }
}
