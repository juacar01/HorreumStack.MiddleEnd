namespace HorreumStack.MiddleEnd.Core.Features.Users;

public class UserDto
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    public string Fullname
    {
        get => Nombre + " " + Apellidos;
    }
}
