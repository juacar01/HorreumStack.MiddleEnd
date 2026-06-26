
using MediatR;

namespace HorreumStack.Identity.Core.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserVm>
{
    public Guid UserId { get; set; }

    public GetUserByIdQuery(Guid id)
    { UserId = id == Guid.Empty ? throw new ArgumentNullException(nameof(id)) : id; }

}
