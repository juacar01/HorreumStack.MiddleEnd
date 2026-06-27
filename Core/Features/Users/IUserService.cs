namespace HorreumStack.MiddleEnd.Core.Features.Users;

public interface IUserService
{
    Task<UserVm> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UserVm> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Guid?> GetUserIdByTokenAsync(string token, CancellationToken cancellationToken);


}