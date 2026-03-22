namespace SDNet.Models.ServiceCatalog
{
    public sealed class ServiceCatalogServiceItem : ServiceCatalogComponent
    {
        public ServiceCatalogServiceItem(
            int id,
            string name,
            string code,
            string description,
            string fulfillmentGroup,
            string requestType,
            int estimatedHours)
            : base(id, name, code, description)
        {
            FulfillmentGroup = fulfillmentGroup?.Trim() ?? string.Empty;
            RequestType = requestType?.Trim() ?? string.Empty;
            EstimatedHours = estimatedHours < 0 ? 0 : estimatedHours;
        }

        public string FulfillmentGroup { get; }

        public string RequestType { get; }

        public int EstimatedHours { get; }
    }
}
