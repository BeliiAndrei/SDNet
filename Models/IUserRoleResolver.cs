namespace SDNet.Models
{
    public interface IUserRoleResolver
    {
        bool TryResolveRoleId(string roleName, out int roleId);

        bool TryResolveRoleName(int roleId, out string roleName);
    }
}
