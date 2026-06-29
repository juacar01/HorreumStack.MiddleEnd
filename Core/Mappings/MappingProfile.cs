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
        CreateMap<User, UserVm>()
            .ForMember(dest => dest.Proyectos, opt => opt.MapFrom(src => src.ProyectoUsers.Select(pu => pu.Proyecto)))
            .ReverseMap();
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Proyectos, opt => opt.MapFrom(src => src.ProyectoUsers.Select(pu => pu.Proyecto)))
            .ReverseMap();

        CreateMap<Proyecto, ProyectoDto>().ReverseMap();
        CreateMap<Proyecto, ProyectoResponse>()
            .ForMember(dest => dest.IsMine, opt => opt.MapFrom<IsMineResolver>())
            .ReverseMap();
        CreateMap<Proyecto, ProyectoDetailResponse>().IncludeBase<Proyecto, ProyectoResponse>().ReverseMap();

        CreateMap<Almacen, AlmacenDto>().ReverseMap();
        CreateMap<Almacen, AlmacenResponse>().ReverseMap();
        CreateMap<Almacen, AlmacenDetailResponse>().ReverseMap();

        CreateMap<Ubicacion, UbicacionDto>().ReverseMap();
        CreateMap<Ubicacion, UbicacionResponse>().ReverseMap();

    }
}