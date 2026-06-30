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

    public class InvitacionRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    [HttpPost("{proyectoId}/invitar")]
    public async Task<IActionResult> Invitar(Guid proyectoId, [FromBody] InvitacionRequest model)
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null) return Unauthorized();

        if (string.IsNullOrEmpty(model.Email)) return BadRequest("El correo electrónico es requerido.");
        var email = model.Email.Trim().ToLower();

        var proyecto = await _unitOfWork.Repository<Proyecto>().GetEntityAsync(p => p.Id == proyectoId);
        if (proyecto == null) return NotFound("Proyecto no encontrado.");
        if (proyecto.OwnerId != userId) return Forbid(); // Solo el dueño puede invitar

        // Buscar si el usuario ya está registrado
        var user = await _unitOfWork.Repository<User>().GetEntityAsync(u => u.Email == email);
        if (user != null)
        {
            // Ya registrado. Verificar si ya tiene acceso.
            var existingProyectoUser = await _unitOfWork.Repository<ProyectoUser>().GetEntityAsync(
                pu => pu.ProyectoId == proyectoId && pu.UserId == user.Id
            );

            if (existingProyectoUser == null)
            {
                // Asociar acceso inmediato
                var nuevoProyectoUser = new ProyectoUser
                {
                    ProyectoId = proyectoId,
                    UserId = user.Id,
                    Role = HorreumStack.Domain.Enums.ProyectoUserRole.Invited,
                    Status = HorreumStack.Domain.Enums.ProyectoUserStatus.Active
                };
                _unitOfWork.Repository<ProyectoUser>().AddEntity(nuevoProyectoUser);
                await _unitOfWork.Complete();
            }

            Console.WriteLine($"====================================================");
            Console.WriteLine($"INVITACION DIRECTA A PROYECTO (USUARIO EXISTENTE)");
            Console.WriteLine($"Para: {email}");
            Console.WriteLine($"Mensaje: Has sido invitado al proyecto '{proyecto.Nombre}'. Ya puedes verlo en tu panel.");
            Console.WriteLine($"====================================================");

            return Ok(new { Message = "Acceso otorgado de forma directa al usuario registrado." });
        }
        else
        {
            // No registrado. Crear Invitacion.
            var invToken = Guid.NewGuid().ToString("N");
            var invitacion = new Invitacion
            {
                Id = Guid.NewGuid(),
                ProyectoId = proyectoId,
                Email = email,
                Token = invToken,
                TokenExpiration = DateTime.UtcNow.AddHours(48),
                Accepted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedBy = userId.ToString()
            };

            await _unitOfWork.Repository<Invitacion>().AddAsync(invitacion);
            await _unitOfWork.Complete();

            var signupLink = $"http://localhost:4200/signup?email={Uri.EscapeDataString(email)}";

            Console.WriteLine($"====================================================");
            Console.WriteLine($"INVITACION A PROYECTO (USUARIO NO REGISTRADO)");
            Console.WriteLine($"Para: {email}");
            Console.WriteLine($"Enlace de Registro e Invitacion: {signupLink}");
            Console.WriteLine($"====================================================");

            return Ok(new { Message = "Invitación generada. El usuario recibirá un enlace para registrarse." });
        }
    }

    [HttpGet("{proyectoId}/colaboradores")]
    public async Task<IActionResult> GetColaboradores(Guid proyectoId)
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null) return Unauthorized();

        var proyecto = await _unitOfWork.Repository<Proyecto>().GetEntityAsync(p => p.Id == proyectoId);
        if (proyecto == null) return NotFound("Proyecto no encontrado.");
        if (proyecto.OwnerId != userId) return Forbid(); // Solo el dueño puede ver

        var colaboradores = await _unitOfWork.Repository<ProyectoUser>().GetAsync(
            pu => pu.ProyectoId == proyectoId && pu.UserId != userId && pu.Status == HorreumStack.Domain.Enums.ProyectoUserStatus.Active,
            null,
            new List<System.Linq.Expressions.Expression<System.Func<ProyectoUser, object>>> { c => c.User }
        );

        var list = colaboradores.Select(c => new {
            UserId = c.UserId,
            Nombre = c.User.Nombre,
            Apellidos = c.User.Apellidos,
            Email = c.User.Email,
            Role = c.Role.ToString(),
            Status = c.Status.ToString()
        }).ToList();

        return Ok(list);
    }

    [HttpDelete("{proyectoId}/colaboradores/{colaboradorUserId}")]
    public async Task<IActionResult> RevokeAcceso(Guid proyectoId, Guid colaboradorUserId)
    {
        var token = Request.Headers["Authorization"];
        Guid? userId = await _userService.GetUserIdByTokenAsync(token.ToString().Replace("Bearer ", ""), CancellationToken.None);
        if (userId == null) return Unauthorized();

        var proyecto = await _unitOfWork.Repository<Proyecto>().GetEntityAsync(p => p.Id == proyectoId);
        if (proyecto == null) return NotFound("Proyecto no encontrado.");
        if (proyecto.OwnerId != userId) return Forbid(); // Solo el dueño puede revocar

        var proyectoUser = await _unitOfWork.Repository<ProyectoUser>().GetEntityAsync(
            pu => pu.ProyectoId == proyectoId && pu.UserId == colaboradorUserId
        );

        if (proyectoUser == null) return NotFound("Colaborador no encontrado en el proyecto.");

        await _unitOfWork.Repository<ProyectoUser>().DeleteAsync(proyectoUser);
        await _unitOfWork.Complete();

        return Ok(new { Message = "Acceso revocado correctamente." });
    }
}
