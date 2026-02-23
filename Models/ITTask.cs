namespace SDNet.Models
{
    public sealed class ITTask : SDTask
    {
        public string SystemArea { get; set; } = string.Empty;
        public bool RequiresDeployment { get; set; }

        public override string TaskTypeName => SDTaskTypes.ITTask;

        public override object Clone()
        {
            var clone = new ITTask();
            CopyBaseTo(clone);
            clone.SystemArea = SystemArea;
            clone.RequiresDeployment = RequiresDeployment;
            return clone;
        }
    }
}
