using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.Domain.Entities;
using HorreumStack.MiddleEnd.Core.Features.Almacenes;
using HorreumStack.MiddleEnd.Core.Features.Users;


namespace HorreumStack.MiddleEnd.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AlmacenesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAlmacenService _almacenService;
    private readonly IUserService _userService;

    public AlmacenesController(IUnitOfWork unitOfWork, IAlmacenService almacenService, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _almacenService = almacenService;
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
        var almacenes = await _almacenService.GetListByUserIdAsync(userId.Value);
        return Ok(almacenes);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var almacenes = await _unitOfWork.Repository<Almacen>().GetAllAsync();
        return Ok(almacenes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var almacen = await _unitOfWork.Repository<Almacen>().GetEntityAsync(a => a.Id == id);
        if (almacen == null)
        {
            return NotFound("Almacén no encontrado.");
        }
        return Ok(almacen);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] AlmacenDto model)
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

        var almacen = await _almacenService.CreateAsync(model, userId.Value);

        return CreatedAtAction(nameof(GetById), new { id = almacen.Id }, almacen);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AlmacenDto model)
    {
        if (string.IsNullOrEmpty(model.Codigo) || string.IsNullOrEmpty(model.Nombre))
        {
            return BadRequest("El código y el nombre son obligatorios.");
        }

        var almacen = await _unitOfWork.Repository<Almacen>().GetEntityAsync(a => a.Id == id);
        if (almacen == null)
        {
            return NotFound("Almacén no encontrado.");
        }

        almacen.Codigo = model.Codigo;
        almacen.Nombre = model.Nombre;
        almacen.LastModifiedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Almacen>().UpdateAsync(almacen);
        await _unitOfWork.Complete();

        return Ok(almacen);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var almacen = await _unitOfWork.Repository<Almacen>().GetEntityAsync(a => a.Id == id);
        if (almacen == null)
        {
            return NotFound("Almacén no encontrado.");
        }

        await _unitOfWork.Repository<Almacen>().DeleteAsync(almacen);
        await _unitOfWork.Complete();

        return Ok(new { Message = "Almacén eliminado correctamente." });
    }
}
