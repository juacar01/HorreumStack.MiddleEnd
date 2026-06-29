using HorreumStack.MiddleEnd.Core.Features.Almacenes;


namespace HorreumStack.MiddleEnd.Core.Features.Proyectos;

public class ProyectoDetailResponse : ProyectoResponse
{
    public List<AlmacenResponse> Almacenes { get; set; } = new List<AlmacenResponse>();
    //public List<UserDto> Invitados { get; set; } = new List<UserDto>();
}

