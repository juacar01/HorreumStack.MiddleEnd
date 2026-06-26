using AutoMapper;
using HorreumStack.Domain.Entities;
using HorreumStack.Infrastructure.Repositories;
using MediatR;
using System.Linq.Expressions;

namespace HorreumStack.Identity.Core.Application.Features.Users.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserVm>
{
    private IUnitOfWork _unitOfWork;
    private IMapper _mapper;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserVm> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var includes = new List<Expression<Func<User, object>>>();

        var user = await _unitOfWork.Repository<User>().GetEntityAsync(
            a => a.Id == request.UserId,
            includes,
            true
        );

        return _mapper.Map<UserVm>(user);
    }
}
