using SDNet.Models;

namespace SDNet.Services
{
    public interface IReferenceCatalogAdminService
    {
        IReadOnlyList<ReferenceValue> GetDepartments();

        IReadOnlyList<ReferenceValue> GetQueryTypes();

        IReadOnlyList<ReferenceValue> GetItProjects();

        ReferenceValue AddDepartment(string name, string code);

        ReferenceValue AddQueryType(string name, string code);

        ReferenceValue AddItProject(string name, string code);

        void DeleteDepartment(int id);

        void DeleteQueryType(int id);

        void DeleteItProject(int id);
    }
}
