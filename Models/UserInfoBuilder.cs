namespace SDNet.Models
{
    public sealed class UserInfoBuilder : UserInfoBuilderBase
    {
        public UserInfoBuilder(IUserRoleResolver? roleResolver = null)
            : base(roleResolver)
        {
        }

        protected override void Validate(UserInfo candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate.UserName))
            {
                throw new InvalidOperationException("UserName is required.");
            }

            if (string.IsNullOrWhiteSpace(candidate.UserFullName))
            {
                throw new InvalidOperationException("UserFullName is required.");
            }

            if (candidate.UserRoleId <= 0 && string.IsNullOrWhiteSpace(candidate.UserRoleName))
            {
                throw new InvalidOperationException("Role information is required.");
            }

            if (candidate.UserDepartId <= 0 && string.IsNullOrWhiteSpace(candidate.UserDepartName))
            {
                throw new InvalidOperationException("Department information is required.");
            }
        }
    }
}
