using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Repositories;
using MediatR;
using System.Linq.Expressions;

namespace HorreumStack.MiddleEnd.Core.Features.Users.Queries;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserVm>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserByEmailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserVm> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var includes = new List<Expression<Func<User, object>>>();

        var user = await _unitOfWork.Repository<User>().GetEntityAsync(
            u => u.Email == request.Email,
            includes,
            true
        );

        return _mapper.Map<UserVm>(user);
    }
}
