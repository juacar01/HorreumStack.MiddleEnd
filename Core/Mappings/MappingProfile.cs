using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.MiddleEnd.Core.Features.Almacenes;

namespace HorreumStack.MiddleEnd.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Almacen, AlmacenDto>().ReverseMap();
    }
}