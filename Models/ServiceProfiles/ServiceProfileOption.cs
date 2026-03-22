namespace SDNet.Models.ServiceProfiles
{
    public sealed class ServiceProfileOption
    {
        public int? Id { get; init; }

        public string DisplayName { get; init; } = string.Empty;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
