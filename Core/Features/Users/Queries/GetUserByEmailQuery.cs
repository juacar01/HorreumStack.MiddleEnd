using MediatR;


namespace HorreumStack.Identity.Core.Application.Features.Users.Queries;

public class GetUserByEmailQuery : IRequest<UserVm>
{
    public string Email { get; set; }

    public GetUserByEmailQuery(string email)
    { Email = string.IsNullOrWhiteSpace(email) ? throw new ArgumentNullException(nameof(email)) : email; }
}
