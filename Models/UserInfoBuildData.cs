namespace SDNet.Models
{
    public sealed class UserInfoBuildData
    {
        public int UserId { get; init; }

        public string? UserName { get; init; }

        public string? UserFullName { get; init; }

        public int UserRoleId { get; init; }

        public string? UserRoleName { get; init; }

        public int UserDepartId { get; init; }

        public string? UserDepartName { get; init; }

        public string? Email { get; init; }

        public string? PhoneNumber { get; init; }

        public bool IsActive { get; init; } = true;

        public DateTime? AuthorizedAt { get; init; }

        public DateTime? LastActivityAt { get; init; }

        public static UserInfoBuildData FromUser(UserInfo source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new UserInfoBuildData
            {
                UserId = source.UserId,
                UserName = source.UserName,
                UserFullName = source.UserFullName,
                UserRoleId = source.UserRoleId,
                UserRoleName = source.UserRoleName,
                UserDepartId = source.UserDepartId,
                UserDepartName = source.UserDepartName,
                Email = source.Email,
                PhoneNumber = source.PhoneNumber,
                IsActive = source.IsActive,
                AuthorizedAt = source.AuthorizedAt,
                LastActivityAt = source.LastActivityAt
            };
        }
    }
}
