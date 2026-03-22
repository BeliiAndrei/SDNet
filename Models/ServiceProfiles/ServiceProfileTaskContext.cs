namespace SDNet.Models.ServiceProfiles
{
    public sealed class ServiceProfileTaskContext
    {
        public string SelectedTaskType { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string QueryTypeName { get; set; } = string.Empty;

        public string ItProjectName { get; set; } = string.Empty;

        public string UserQueryTag { get; set; } = string.Empty;

        public string PerformerDepartName { get; set; } = string.Empty;

        public string ShortDescription { get; set; } = string.Empty;

        public DateTime DateReg { get; set; }

        public DateTime DateNeedClose { get; set; }
    }
}
