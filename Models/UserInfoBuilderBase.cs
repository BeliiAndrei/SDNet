namespace SDNet.Models
{
    public abstract class UserInfoBuilderBase : IUserInfoBuilder
    {
        private static readonly IUserRoleResolver EmptyRoleResolver = new NullUserRoleResolver();
        private readonly IUserRoleResolver _roleResolver;

        private int _userId;
        private string _userName = string.Empty;
        private string _userFullName = string.Empty;
        private int _userRoleId;
        private string _userRoleName = string.Empty;
        private int _userDepartId;
        private string _userDepartName = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private bool _isActive = true;
        private DateTime? _authorizedAt;
        private DateTime? _lastActivityAt;

        protected UserInfoBuilderBase(IUserRoleResolver? roleResolver = null)
        {
            _roleResolver = roleResolver ?? EmptyRoleResolver;
            Reset();
        }

        public IUserInfoBuilder Reset()
        {
            _userId = 0;
            _userName = string.Empty;
            _userFullName = string.Empty;
            _userRoleId = 0;
            _userRoleName = string.Empty;
            _userDepartId = 0;
            _userDepartName = string.Empty;
            _email = string.Empty;
            _phoneNumber = string.Empty;
            _isActive = true;
            _authorizedAt = null;
            _lastActivityAt = null;
            return this;
        }

        public IUserInfoBuilder WithUserId(int userId)
        {
            if (userId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userId), "UserId cannot be negative.");
            }

            _userId = userId;
            return this;
        }

        public IUserInfoBuilder WithUserName(string? userName)
        {
            _userName = Normalize(userName);
            return this;
        }

        public IUserInfoBuilder WithUserFullName(string? userFullName)
        {
            _userFullName = Normalize(userFullName);
            return this;
        }

        public IUserInfoBuilder WithRole(int roleId, string? roleName = null)
        {
            if (roleId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(roleId), "RoleId cannot be negative.");
            }

            _userRoleId = roleId;
            _userRoleName = Normalize(roleName);
            return this;
        }

        public IUserInfoBuilder WithRoleName(string? roleName)
        {
            _userRoleName = Normalize(roleName);
            return this;
        }

        public IUserInfoBuilder WithDepartment(int departId, string? departName = null)
        {
            if (departId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(departId), "DepartmentId cannot be negative.");
            }

            _userDepartId = departId;
            _userDepartName = Normalize(departName);
            return this;
        }

        public IUserInfoBuilder WithDepartmentName(string? departName)
        {
            _userDepartName = Normalize(departName);
            return this;
        }

        public IUserInfoBuilder WithEmail(string? email)
        {
            _email = Normalize(email);
            return this;
        }

        public IUserInfoBuilder WithPhoneNumber(string? phoneNumber)
        {
            _phoneNumber = Normalize(phoneNumber);
            return this;
        }

        public IUserInfoBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public IUserInfoBuilder WithAuthorizedAt(DateTime? authorizedAt)
        {
            _authorizedAt = authorizedAt;
            return this;
        }

        public IUserInfoBuilder WithLastActivityAt(DateTime? lastActivityAt)
        {
            _lastActivityAt = lastActivityAt;
            return this;
        }

        public UserInfo Build()
        {
            ResolveRole();

            var candidate = new UserInfo
            {
                UserId = _userId,
                UserName = _userName,
                UserFullName = _userFullName,
                UserRoleId = _userRoleId,
                UserRoleName = _userRoleName,
                UserDepartId = _userDepartId,
                UserDepartName = _userDepartName,
                Email = _email,
                PhoneNumber = _phoneNumber,
                IsActive = _isActive,
                AuthorizedAt = _authorizedAt is null || _authorizedAt == default ? DateTime.Now : _authorizedAt.Value,
                LastActivityAt = _lastActivityAt
            };

            Validate(candidate);
            return candidate;
        }

        protected abstract void Validate(UserInfo candidate);

        private static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private void ResolveRole()
        {
            if (_userRoleId <= 0 &&
                !string.IsNullOrWhiteSpace(_userRoleName) &&
                _roleResolver.TryResolveRoleId(_userRoleName, out int roleId))
            {
                _userRoleId = roleId;
            }

            if (string.IsNullOrWhiteSpace(_userRoleName) &&
                _userRoleId > 0 &&
                _roleResolver.TryResolveRoleName(_userRoleId, out string roleName))
            {
                _userRoleName = Normalize(roleName);
            }
        }

        private sealed class NullUserRoleResolver : IUserRoleResolver
        {
            public bool TryResolveRoleId(string roleName, out int roleId)
            {
                roleId = 0;
                return false;
            }

            public bool TryResolveRoleName(int roleId, out string roleName)
            {
                roleName = string.Empty;
                return false;
            }
        }
    }
}
