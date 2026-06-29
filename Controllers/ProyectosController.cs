using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.Domain.Entities;
using HorreumStack.MiddleEnd.Core.Features.Users;
using HorreumStack.MiddleEnd.Core.Features.Proyectos;


namespace HorreumStack.MiddleEnd.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProyectosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProyectoService _proyectoService;
    private readonly IUserService _userService;

    public ProyectosController(IUnitOfWork unitOfWork, IProyectoService proyectoService, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _proyectoService = proyectoService;
        _userService = userService;
    }

    [HttpGet("getallforme")]
    public async Task<IActionResult> GetAllForMe()
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null)
        {
            return Unauthorized();
        }
        var proyectos = await _proyectoService.GetListByUserIdAsync(userId.Value);
        return Ok(new { proyectos = proyectos, userId = userId });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var proyectos = await _proyectoService.GetAllAsync();
        return Ok(proyectos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null)
        {
            return Unauthorized();
        }
        var proyecto = await _proyectoService.GetByIdAsync(id, userId.Value);
        if (proyecto == null || proyecto.Codigo == null) return NotFound("Proyecto no encontrado.");

        return Ok(proyecto);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] ProyectoDto model)
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(model.Codigo) || string.IsNullOrEmpty(model.Nombre))
        {
            return BadRequest("El código y el nombre son obligatorios.");
        }

        var proyecto = await _proyectoService.CreateAsync(model, userId.Value);
        if (proyecto.Id == null) return BadRequest("Error al crear el proyecto.");

        var proyectoresult = await _proyectoService.GetByIdAsync((Guid)proyecto.Id, userId.Value);
        if (proyectoresult == null) return BadRequest("Error al obtener el proyecto.");

        return CreatedAtAction(nameof(GetById), new { id = proyectoresult.Id }, proyectoresult);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProyectoDto model)
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null)
        {
            return Unauthorized();
        }
        if (string.IsNullOrEmpty(model.Codigo) || string.IsNullOrEmpty(model.Nombre))
        {
            return BadRequest("El código y el nombre son obligatorios.");
        }

        try
        {
            var updated = await _proyectoService.UpdateAsync(id, model);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Proyecto no encontrado.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {

        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null)
        {
            return Unauthorized();
        }

        var proyecto = await _unitOfWork.Repository<Proyecto>().GetEntityAsync(a => a.Id == id);
        if (proyecto == null)
        {
            return NotFound("Proyecto no encontrado.");
        }

        await _unitOfWork.Repository<Proyecto>().DeleteAsync(proyecto);
        await _unitOfWork.Complete();

        return Ok(new { Message = "Proyecto eliminado correctamente." });
    }
}
