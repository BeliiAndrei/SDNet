namespace SDNet.Models.ServiceCatalog
{
    public sealed class ServiceCatalogCategoryOption
    {
        public int? Id { get; init; }

        public string DisplayName { get; init; } = string.Empty;

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
