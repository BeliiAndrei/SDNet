namespace SDNet.Models.ServiceProfiles
{
    public interface IServiceProfileFlyweight
    {
        int Id { get; }

        int ServiceCatalogNodeId { get; }

        string ServiceCode { get; }

        string ServiceName { get; }

        string ServiceDescription { get; }

        string FulfillmentGroup { get; }

        string RequestType { get; }

        int EstimatedHours { get; }

        string DefaultTaskTypeName { get; }

        string DefaultPriority { get; }

        string DefaultQueryTypeName { get; }

        string DefaultItProjectName { get; }

        string DefaultUserQueryTag { get; }

        string DefaultPerformerDepartName { get; }

        string DefaultShortDescription { get; }

        int SlaHours { get; }

        void ApplyTo(ServiceProfileTaskContext context);
    }
}
