using HorreumStack.MiddleEnd.Core.Features.Proyectos;

namespace HorreumStack.MiddleEnd.Core.Features.Users;

public class UserDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    public virtual List<ProyectoDto> Proyectos { get; set; } = new List<ProyectoDto>();

    public string Fullname
    {
        get => Nombre + " " + Apellidos;
    }
}
