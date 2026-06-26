using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.Domain.Entities;

namespace HorreumStack.MiddleEnd.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AlmacenesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AlmacenesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AlmacenDto model)
    {
        if (string.IsNullOrEmpty(model.Codigo) || string.IsNullOrEmpty(model.Nombre))
        {
            return BadRequest("El código y el nombre son obligatorios.");
        }

        var almacen = new Almacen
        {
            Codigo = model.Codigo,
            Nombre = model.Nombre
        };

        await _unitOfWork.Repository<Almacen>().AddAsync(almacen);
        await _unitOfWork.Complete();

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

public class AlmacenDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
