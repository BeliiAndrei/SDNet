namespace SDNet.Models
{
    public sealed class IntegrationTask : SDTask
    {
        public string EndpointName { get; set; } = string.Empty;
        public string IntegrationSystem { get; set; } = string.Empty;

        public override string TaskTypeName => SDTaskTypes.IntegrationTask;

        public override object Clone()
        {
            var clone = new IntegrationTask();
            CopyBaseTo(clone);
            clone.EndpointName = EndpointName;
            clone.IntegrationSystem = IntegrationSystem;
            return clone;
        }
    }
}
