namespace SDNet.Models
{
    public sealed class UserInfoDirector
    {
        public UserInfo BuildForSave(IUserInfoBuilder builder, UserInfoBuildData data)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(data);

            return builder
                .Reset()
                .WithUserId(data.UserId)
                .WithUserName(data.UserName)
                .WithUserFullName(data.UserFullName)
                .WithRole(data.UserRoleId, data.UserRoleName)
                .WithDepartment(data.UserDepartId, data.UserDepartName)
                .WithEmail(data.Email)
                .WithPhoneNumber(data.PhoneNumber)
                .WithIsActive(data.IsActive)
                .Build();
        }

        public UserInfo BuildFromDatabase(IUserInfoBuilder builder, UserInfoBuildData data)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(data);

            return builder
                .Reset()
                .WithUserId(data.UserId)
                .WithUserName(data.UserName)
                .WithUserFullName(data.UserFullName)
                .WithRole(data.UserRoleId, data.UserRoleName)
                .WithDepartment(data.UserDepartId, data.UserDepartName)
                .WithEmail(data.Email)
                .WithPhoneNumber(data.PhoneNumber)
                .WithIsActive(data.IsActive)
                .WithAuthorizedAt(data.AuthorizedAt)
                .WithLastActivityAt(data.LastActivityAt)
                .Build();
        }
    }
}
