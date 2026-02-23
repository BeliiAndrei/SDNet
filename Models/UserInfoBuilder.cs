namespace SDNet.Models
{
    public sealed class UserInfoBuilder
    {
        private const int AdministratorRoleId = 1;
        private const int UserRoleId = 2;
        private const string AdministratorRoleName = "Administrator";
        private const string UserRoleName = "User";

        private readonly UserInfo _user = new();

        private UserInfoBuilder()
        {
        }

        public static UserInfoBuilder Create()
        {
            return new UserInfoBuilder();
        }

        public static UserInfoBuilder From(UserInfo source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return Create()
                .WithUserId(source.UserId)
                .WithUserName(source.UserName)
                .WithUserFullName(source.UserFullName)
                .WithRole(source.UserRoleId, source.UserRoleName)
                .WithDepartment(source.UserDepartId, source.UserDepartName)
                .WithEmail(source.Email)
                .WithPhoneNumber(source.PhoneNumber)
                .WithIsActive(source.IsActive)
                .WithAuthorizedAt(source.AuthorizedAt)
                .WithLastActivityAt(source.LastActivityAt);
        }

        public UserInfoBuilder WithUserId(int userId)
        {
            _user.UserId = Math.Max(userId, 0);
            return this;
        }

        public UserInfoBuilder WithUserName(string? userName)
        {
            _user.UserName = Normalize(userName);
            return this;
        }

        public UserInfoBuilder WithUserFullName(string? userFullName)
        {
            _user.UserFullName = Normalize(userFullName);
            return this;
        }

        public UserInfoBuilder WithRole(int roleId, string? roleName = null)
        {
            _user.UserRoleId = Math.Max(roleId, 0);
            _user.UserRoleName = Normalize(roleName);
            return this;
        }

        public UserInfoBuilder WithRoleName(string? roleName)
        {
            _user.UserRoleName = Normalize(roleName);
            return this;
        }

        public UserInfoBuilder WithDepartment(int departId, string? departName = null)
        {
            _user.UserDepartId = Math.Max(departId, 0);
            _user.UserDepartName = Normalize(departName);
            return this;
        }

        public UserInfoBuilder WithDepartmentName(string? departName)
        {
            _user.UserDepartName = Normalize(departName);
            return this;
        }

        public UserInfoBuilder WithEmail(string? email)
        {
            _user.Email = Normalize(email);
            return this;
        }

        public UserInfoBuilder WithPhoneNumber(string? phoneNumber)
        {
            _user.PhoneNumber = Normalize(phoneNumber);
            return this;
        }

        public UserInfoBuilder WithIsActive(bool isActive)
        {
            _user.IsActive = isActive;
            return this;
        }

        public UserInfoBuilder WithAuthorizedAt(DateTime authorizedAt)
        {
            _user.AuthorizedAt = authorizedAt;
            return this;
        }

        public UserInfoBuilder WithLastActivityAt(DateTime? lastActivityAt)
        {
            _user.LastActivityAt = lastActivityAt;
            return this;
        }

        public UserInfo Build()
        {
            var user = new UserInfo
            {
                UserId = _user.UserId,
                UserFullName = Normalize(_user.UserFullName),
                UserName = Normalize(_user.UserName),
                UserRoleId = _user.UserRoleId,
                UserRoleName = Normalize(_user.UserRoleName),
                UserDepartId = _user.UserDepartId,
                UserDepartName = Normalize(_user.UserDepartName),
                Email = Normalize(_user.Email),
                PhoneNumber = Normalize(_user.PhoneNumber),
                IsActive = _user.IsActive,
                AuthorizedAt = _user.AuthorizedAt == default ? DateTime.Now : _user.AuthorizedAt,
                LastActivityAt = _user.LastActivityAt
            };

            user.UserRoleId = NormalizeRoleId(user.UserRoleId, user.UserRoleName);
            user.UserRoleName = NormalizeRoleName(user.UserRoleId, user.UserRoleName);
            return user;
        }

        private static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static int NormalizeRoleId(int roleId, string roleName)
        {
            if (roleId > 0)
            {
                return roleId;
            }

            if (string.Equals(roleName, AdministratorRoleName, StringComparison.OrdinalIgnoreCase))
            {
                return AdministratorRoleId;
            }

            return UserRoleId;
        }

        private static string NormalizeRoleName(int roleId, string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                return roleName;
            }

            return roleId == AdministratorRoleId ? AdministratorRoleName : UserRoleName;
        }
    }
}
