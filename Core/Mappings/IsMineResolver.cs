using AutoMapper;
using Microsoft.AspNetCore.Http;
using HorreumStack.Domain.Entities;
using HorreumStack.MiddleEnd.Core.Features.Proyectos;
using HorreumStack.Utilities.Security;

namespace HorreumStack.MiddleEnd.Core.Mappings;

public class IsMineResolver : IValueResolver<Proyecto, ProyectoResponse, bool?>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtHelper _jwtHelper;

    public IsMineResolver(IHttpContextAccessor httpContextAccessor, IJwtHelper jwtHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtHelper = jwtHelper;
    }

    public bool? Resolve(Proyecto source, ProyectoResponse destination, bool? destMember, ResolutionContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return false;

        string? authorizationHeader = httpContext.Request.Headers["Authorization"];
        if (string.IsNullOrEmpty(authorizationHeader)) return false;

        var token = authorizationHeader.Replace("Bearer ", "");
        var userIdStr = _jwtHelper.GetUserIdFromToken(token);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return false;
        }

        return source.OwnerId == userId;
    }
}
