using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.MiddleEnd.Core.Features.Almacenes;
using HorreumStack.MiddleEnd.Core.Features.Users;
using HorreumStack.MiddleEnd.Core.Features.Ubicaciones;
using HorreumStack.MiddleEnd.Core.Features.UbicacionesTipos;
using HorreumStack.MiddleEnd.Core.Features.ItemTipos;
using HorreumStack.MiddleEnd.Core.Features.Items;
using HorreumStack.MiddleEnd.Core.Features.Proyectos;
using HorreumStack.MiddleEnd.Core.Features.Inventarios;

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
        CreateMap<Ubicacion, UbicacionDetailResponse>().ReverseMap();

        CreateMap<UbicacionTipo, UbicacionTipoDto>().ReverseMap();
        CreateMap<UbicacionTipo, UbicacionTipoResponse>().ReverseMap();
        CreateMap<UbicacionTipo, UbicacionTipoComboResponse>().ReverseMap();

        CreateMap<ItemTipo, ItemTipoDto>().ReverseMap();
        CreateMap<ItemTipo, ItemTipoResponse>().ReverseMap();
        CreateMap<ItemTipo, ItemTipoComboResponse>().ReverseMap();

        CreateMap<Item, ItemDto>().ReverseMap();
        CreateMap<Item, ItemResponse>().ReverseMap();

        CreateMap<Inventario, InventarioResponse>().ReverseMap();
        CreateMap<InventarioMovimiento, InventarioMovimientoResponse>()
            .ForMember(dest => dest.UbicacionOrigenNombre, opt => opt.MapFrom(src => src.UbicacionOrigen != null ? src.UbicacionOrigen.Nombre : string.Empty))
            .ForMember(dest => dest.UbicacionDestinoNombre, opt => opt.MapFrom(src => src.UbicacionDestino != null ? src.UbicacionDestino.Nombre : string.Empty))
            .ReverseMap();
    }
}