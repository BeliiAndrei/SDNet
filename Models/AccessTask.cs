namespace SDNet.Models
{
    public sealed class AccessTask : SDTask
    {
        public string AccessRole { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;

        public override string TaskTypeName => SDTaskTypes.AccessTask;

        public override object Clone()
        {
            var clone = new AccessTask();
            CopyBaseTo(clone);
            clone.AccessRole = AccessRole;
            clone.ResourceName = ResourceName;
            return clone;
        }
    }
}
