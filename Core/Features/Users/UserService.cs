using System.Linq.Expressions;
using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Repositories;
using HorreumStack.Utilities.Security;

namespace HorreumStack.MiddleEnd.Core.Features.Users;

public class UserService : IUserService
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IJwtHelper _jwtHelper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper, IJwtHelper jwtHelper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwtHelper = jwtHelper;
    }

    public async Task<UserVm> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var includes = new List<Expression<Func<User, object>>>();

        var user = await _unitOfWork.Repository<User>().GetEntityAsync(
            a => a.Id == id,
            includes,
            true
        );

        return _mapper.Map<UserVm>(user);
    }

    public async Task<UserVm> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var includes = new List<Expression<Func<User, object>>>();

        var user = await _unitOfWork.Repository<User>().GetEntityAsync(
            u => u.Email == email,
            includes,
            true
        );

        return _mapper.Map<UserVm>(user);
    }

    public async Task<Guid?> GetUserIdByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var userIdStr = _jwtHelper.GetUserIdFromToken(token);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return null;
        }
        var user = await GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return null;
        }
        return user.Id;
    }

}