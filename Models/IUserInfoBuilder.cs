namespace SDNet.Models
{
    public interface IUserInfoBuilder
    {
        IUserInfoBuilder Reset();

        IUserInfoBuilder WithUserId(int userId);

        IUserInfoBuilder WithUserName(string? userName);

        IUserInfoBuilder WithUserFullName(string? userFullName);

        IUserInfoBuilder WithRole(int roleId, string? roleName = null);

        IUserInfoBuilder WithRoleName(string? roleName);

        IUserInfoBuilder WithDepartment(int departId, string? departName = null);

        IUserInfoBuilder WithDepartmentName(string? departName);

        IUserInfoBuilder WithEmail(string? email);

        IUserInfoBuilder WithPhoneNumber(string? phoneNumber);

        IUserInfoBuilder WithIsActive(bool isActive);

        IUserInfoBuilder WithAuthorizedAt(DateTime? authorizedAt);

        IUserInfoBuilder WithLastActivityAt(DateTime? lastActivityAt);

        UserInfo Build();
    }
}
