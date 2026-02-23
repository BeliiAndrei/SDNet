namespace SDNet.Models
{
    public sealed class SecurityTask : SDTask
    {
        public string RiskLevel { get; set; } = string.Empty;
        public bool RequiresAudit { get; set; }

        public override string TaskTypeName => SDTaskTypes.SecurityTask;

        public override object Clone()
        {
            var clone = new SecurityTask();
            CopyBaseTo(clone);
            clone.RiskLevel = RiskLevel;
            clone.RequiresAudit = RequiresAudit;
            return clone;
        }
    }
}
