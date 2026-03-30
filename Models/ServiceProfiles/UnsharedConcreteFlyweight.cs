namespace SDNet.Models.ServiceProfiles
{
    public sealed class UnsharedConcreteFlyweight : IServiceProfileFlyweight
    {
        public UnsharedConcreteFlyweight(IServiceProfileFlyweight source)
        {
            ArgumentNullException.ThrowIfNull(source);

            Id = source.Id;
            ServiceCatalogNodeId = source.ServiceCatalogNodeId;
            ServiceCode = source.ServiceCode;
            ServiceName = source.ServiceName;
            ServiceDescription = source.ServiceDescription;
            FulfillmentGroup = source.FulfillmentGroup;
            RequestType = source.RequestType;
            EstimatedHours = source.EstimatedHours;
            DefaultTaskTypeName = source.DefaultTaskTypeName;
            DefaultPriority = source.DefaultPriority;
            DefaultQueryTypeName = source.DefaultQueryTypeName;
            DefaultItProjectName = source.DefaultItProjectName;
            DefaultUserQueryTag = source.DefaultUserQueryTag;
            DefaultPerformerDepartName = source.DefaultPerformerDepartName;
            DefaultShortDescription = source.DefaultShortDescription;
            SlaHours = source.SlaHours;
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
