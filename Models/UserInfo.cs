namespace SDNet.Models
{
    public sealed class UserInfo
    {
        public int UserId { get; set; }

        public string UserFullName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public int UserRoleId { get; set; }

        public string UserRoleName { get; set; } = string.Empty;

        public int UserDepartId { get; set; }

        public string UserDepartName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime AuthorizedAt { get; set; } = DateTime.Now;

        public DateTime? LastActivityAt { get; set; }
    }
}
