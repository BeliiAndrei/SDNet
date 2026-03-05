namespace SDNet.Models
{
    public sealed class DbUserInfoBuilder : UserInfoBuilderBase
    {
        public DbUserInfoBuilder(IUserRoleResolver? roleResolver = null)
            : base(roleResolver)
        {
        }

        protected override void Validate(UserInfo candidate)
        {
            if (candidate.UserId < 0)
            {
                throw new InvalidOperationException("UserId cannot be negative.");
            }
        }
    }
}
