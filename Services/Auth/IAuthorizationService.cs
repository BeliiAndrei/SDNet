using SDNet.Models;

namespace SDNet.Services.Auth
{
    public interface IAuthorizationService
    {
        Task<UserInfo> AuthorizeAsync(string login, string password, CancellationToken cancellationToken = default);
    }
}
