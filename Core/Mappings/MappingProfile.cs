using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.MiddleEnd.Core.Features.Almacenes;
using HorreumStack.MiddleEnd.Core.Features.Users;
using HorreumStack.MiddleEnd.Core.Features.Ubicaciones;

namespace HorreumStack.MiddleEnd.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Almacen, AlmacenDto>().ReverseMap();
        CreateMap<User, UserVm>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Ubicacion, UbicacionDto>().ReverseMap();
        CreateMap<Almacen, AlmacenResponse>().ReverseMap();

    }
}