using HorreumStack.MiddleEnd.Core.Features.Ubicaciones;
using HorreumStack.MiddleEnd.Core.Features.Users;

namespace HorreumStack.MiddleEnd.Core.Features.Almacenes;

public class AlmacenResponse : AlmacenDto
{
    public List<AlmacenDto> SubAlmacenes { get; set; } = new List<AlmacenDto>();
    public List<UserDto> Invitados { get; set; } = new List<UserDto>();
}

