namespace SDNet.Models
{
    public sealed class CommunicationTask : SDTask
    {
        public string Channel { get; set; } = string.Empty;
        public string ContactPoint { get; set; } = string.Empty;

        public override string TaskTypeName => SDTaskTypes.CommunicationTask;

        public override object Clone()
        {
            var clone = new CommunicationTask();
            CopyBaseTo(clone);
            clone.Channel = Channel;
            clone.ContactPoint = ContactPoint;
            return clone;
        }
    }
}
