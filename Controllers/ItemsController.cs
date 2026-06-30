using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HorreumStack.MiddleEnd.Core.Features.Items;
using HorreumStack.MiddleEnd.Core.Features.Users;

namespace HorreumStack.MiddleEnd.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly IUserService _userService;

    public ItemsController(IItemService itemService, IUserService userService)
    {
        _itemService = itemService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _itemService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("proyecto/{proyectoId}")]
    public async Task<IActionResult> GetAllByProyectoId(
        Guid proyectoId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _itemService.GetPaginatedByProyectoIdAsync(proyectoId, pageIndex, pageSize, search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _itemService.GetByIdAsync(id);
        if (result == null)
            return NotFound("Item no encontrado.");

        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] ItemDto model)
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

        var result = await _itemService.CreateAsync(model);
        if (result.Id == Guid.Empty)
            return BadRequest("Error al crear el item.");

        var finalResult = await _itemService.GetByIdAsync(result.Id);
        if (finalResult == null)
            return BadRequest("Error al obtener el item.");

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, finalResult);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ItemDto model)
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
            var updated = await _itemService.UpdateAsync(id, model);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Item no encontrado.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _itemService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound("Item no encontrado.");
        }

        return Ok(new { Message = "Item eliminado correctamente." });
    }

    [HttpPost("proyecto/{proyectoId}/import")]
    public async Task<IActionResult> ImportItems(Guid proyectoId, IFormFile file)
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null)
        {
            return Unauthorized();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No se ha subido ningún archivo.");
        }

        var extension = Path.GetExtension(file.FileName);
        var allowedExtensions = new[] { ".xlsx", ".xls", ".csv" };
        if (!allowedExtensions.Contains(extension.ToLower()))
        {
            return BadRequest("Formato de archivo no válido. Solo se admiten archivos Excel (.xlsx, .xls) o CSV (.csv).");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var count = await _itemService.ImportItemsAsync(proyectoId, stream, extension, userId.Value.ToString());
                return Ok(new { Count = count, Message = $"Se importaron {count} items correctamente." });
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Error al procesar el archivo: {ex.Message}");
        }
    }
}
