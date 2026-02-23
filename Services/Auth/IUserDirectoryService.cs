using SDNet.Models;

namespace SDNet.Services.Auth
{
    public interface IUserDirectoryService
    {
        IReadOnlyList<UserInfo> GetAllUsers();

        UserInfo? GetByLogin(string login);

        UserInfo? GetByFullName(string fullName);

        IReadOnlyList<UserInfo> GetAssignableUsers(UserInfo? currentUser);

        UserInfo Save(UserInfo user);
    }
}
