using HorreumStack.MiddleEnd.Core.Features.Proyectos;

namespace HorreumStack.MiddleEnd.Core.Features.Users;

public class UserVm
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string Email { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<ProyectoDto> Proyectos { get; set; } = new List<ProyectoDto>();

    public string Fullname
    {
        get => Nombre + " " + Apellidos;
    }
}