
using MediatR;

namespace HorreumStack.MiddleEnd.Core.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserVm>
{
    public Guid UserId { get; set; }

    public GetUserByIdQuery(Guid id)
    { UserId = id == Guid.Empty ? throw new ArgumentNullException(nameof(id)) : id; }

}
