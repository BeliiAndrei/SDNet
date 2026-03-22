namespace SDNet.Models.ServiceProfiles
{
    public sealed class ServiceProfileFlyweight : IServiceProfileFlyweight
    {
        public ServiceProfileFlyweight(
            int id,
            int serviceCatalogNodeId,
            string serviceCode,
            string serviceName,
            string serviceDescription,
            string fulfillmentGroup,
            string requestType,
            int estimatedHours,
            string defaultTaskTypeName,
            string defaultPriority,
            string defaultQueryTypeName,
            string defaultItProjectName,
            string defaultUserQueryTag,
            string defaultPerformerDepartName,
            string defaultShortDescription,
            int slaHours)
        {
            Id = id;
            ServiceCatalogNodeId = serviceCatalogNodeId;
            ServiceCode = serviceCode;
            ServiceName = serviceName;
            ServiceDescription = serviceDescription;
            FulfillmentGroup = fulfillmentGroup;
            RequestType = requestType;
            EstimatedHours = estimatedHours;
            DefaultTaskTypeName = defaultTaskTypeName;
            DefaultPriority = defaultPriority;
            DefaultQueryTypeName = defaultQueryTypeName;
            DefaultItProjectName = defaultItProjectName;
            DefaultUserQueryTag = defaultUserQueryTag;
            DefaultPerformerDepartName = defaultPerformerDepartName;
            DefaultShortDescription = defaultShortDescription;
            SlaHours = slaHours;
        }

        public int Id { get; }

        public int ServiceCatalogNodeId { get; }

        public string ServiceCode { get; }

        public string ServiceName { get; }

        public string ServiceDescription { get; }

        public string FulfillmentGroup { get; }

        public string RequestType { get; }

        public int EstimatedHours { get; }

        public string DefaultTaskTypeName { get; }

        public string DefaultPriority { get; }

        public string DefaultQueryTypeName { get; }

        public string DefaultItProjectName { get; }

        public string DefaultUserQueryTag { get; }

        public string DefaultPerformerDepartName { get; }

        public string DefaultShortDescription { get; }

        public int SlaHours { get; }

        public void ApplyTo(ServiceProfileTaskContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!string.IsNullOrWhiteSpace(DefaultTaskTypeName))
            {
                context.SelectedTaskType = DefaultTaskTypeName;
            }

            if (!string.IsNullOrWhiteSpace(DefaultPriority))
            {
                context.Priority = DefaultPriority;
            }

            if (!string.IsNullOrWhiteSpace(DefaultQueryTypeName))
            {
                context.QueryTypeName = DefaultQueryTypeName;
            }

            if (!string.IsNullOrWhiteSpace(DefaultItProjectName))
            {
                context.ItProjectName = DefaultItProjectName;
            }

            if (!string.IsNullOrWhiteSpace(DefaultUserQueryTag))
            {
                context.UserQueryTag = DefaultUserQueryTag;
            }

            if (!string.IsNullOrWhiteSpace(DefaultPerformerDepartName))
            {
                context.PerformerDepartName = DefaultPerformerDepartName;
            }

            if (!string.IsNullOrWhiteSpace(DefaultShortDescription))
            {
                context.ShortDescription = DefaultShortDescription;
            }

            if (SlaHours > 0)
            {
                DateTime referenceDate = context.DateReg == default ? DateTime.Now : context.DateReg;
                context.DateNeedClose = referenceDate.AddHours(SlaHours);
            }
        }
    }
}
