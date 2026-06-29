using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.MiddleEnd.Core.Features.Almacenes;
using HorreumStack.MiddleEnd.Core.Features.Users;
using HorreumStack.MiddleEnd.Core.Features.Ubicaciones;
using HorreumStack.MiddleEnd.Core.Features.Proyectos;

namespace HorreumStack.MiddleEnd.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserVm>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();

        CreateMap<Proyecto, ProyectoDto>().ReverseMap();
        CreateMap<Proyecto, ProyectoResponse>().ReverseMap();
        CreateMap<Proyecto, ProyectoDetailResponse>().ReverseMap();

        CreateMap<Almacen, AlmacenDto>().ReverseMap();
        CreateMap<Almacen, AlmacenResponse>().ReverseMap();
        CreateMap<Almacen, AlmacenDetailResponse>().ReverseMap();

        CreateMap<Ubicacion, UbicacionDto>().ReverseMap();
        CreateMap<Ubicacion, UbicacionResponse>().ReverseMap();

    }
}