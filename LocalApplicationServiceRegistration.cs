using AutoMapper;
using HorreumStack.MiddleEnd.Core.Features.Almacenes;
using HorreumStack.MiddleEnd.Core.Features.Proyectos;
using HorreumStack.MiddleEnd.Core.Features.Users;
using HorreumStack.MiddleEnd.Core.Features.Ubicaciones;
using HorreumStack.MiddleEnd.Core.Features.UbicacionesTipos;
using HorreumStack.MiddleEnd.Core.Features.ItemTipos;
using HorreumStack.MiddleEnd.Core.Features.Items;
using HorreumStack.MiddleEnd.Core.Features.Inventarios;
using HorreumStack.MiddleEnd.Core.Mappings;
using Microsoft.Extensions.Logging.Abstractions;



namespace HorreumStack.MiddleEnd.Core.Application;

public static class ApplicationServiceRegistration
{

    public static IServiceCollection AddLocalApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            // Add your AutoMapper profiles here
            cfg.AddProfile(new MappingProfile());
        }, NullLoggerFactory.Instance);

        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);

        services.AddScoped<IAlmacenService, AlmacenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProyectoService, ProyectoService>();
        services.AddScoped<IUbicacionService, UbicacionService>();
        services.AddScoped<IUbicacionTipoService, UbicacionTipoService>();
        services.AddScoped<IItemTipoService, ItemTipoService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IInventarioService, InventarioService>();


        return services;
    }

}