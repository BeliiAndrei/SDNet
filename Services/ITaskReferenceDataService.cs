namespace SDNet.Services
{
    public interface ITaskReferenceDataService
    {
        IReadOnlyList<string> GetDepartments();

        IReadOnlyList<string> GetQueryTypes();

        IReadOnlyList<string> GetItProjects();

        void InvalidateCache();
    }
}
