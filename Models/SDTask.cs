namespace SDNet.Models
{
    public abstract class SDTask : ICloneable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int UserQueryId { get; set; }
        public DateTime DateReg { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string UserFio { get; set; } = string.Empty;
        public string UserDepartName { get; set; } = string.Empty;
        public string UserQueryTag { get; set; } = string.Empty;
        public string QueryTypeName { get; set; } = string.Empty;
        public string ItProjectName { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public DateTime DateNeedClose { get; set; }
        public string PerformerName { get; set; } = string.Empty;
        public string PerformerDepartName { get; set; } = string.Empty;
        public int PerformPercent { get; set; }
        public DateTime? DateClosed { get; set; }
        public List<string> Notes { get; set; } = [];

        public abstract string TaskTypeName { get; }
        public abstract object Clone();

        protected void CopyBaseTo(SDTask target)
        {
            target.Id = Id;
            target.UserQueryId = UserQueryId;
            target.DateReg = DateReg;
            target.Priority = Priority;
            target.UserFio = UserFio;
            target.UserDepartName = UserDepartName;
            target.UserQueryTag = UserQueryTag;
            target.QueryTypeName = QueryTypeName;
            target.ItProjectName = ItProjectName;
            target.ShortDescription = ShortDescription;
            target.StateName = StateName;
            target.DateNeedClose = DateNeedClose;
            target.PerformerName = PerformerName;
            target.PerformerDepartName = PerformerDepartName;
            target.PerformPercent = PerformPercent;
            target.DateClosed = DateClosed;
            target.Notes = [.. Notes];
        }
    }
}
