using SDNet.Models.ServiceCatalog;

namespace SDNet.Services.ServiceCatalog
{
    public interface IServiceCatalogAdminService
    {
        ServiceCatalogCategory AddCategory(
            string name,
            string code,
            string description,
            int? parentCategoryId);

        void AddService(
            int parentCategoryId,
            string name,
            string code,
            string description,
            string fulfillmentGroup,
            string requestType,
            int estimatedHours,
            string defaultTaskTypeName,
            string defaultPriority,
            string defaultQueryTypeName,
            string defaultItProjectName,
            string defaultUserQueryTag,
            string defaultPerformerDepartName,
            string defaultShortDescription,
            int slaHours);

        void UpdateCategory(
            int id,
            string name,
            string code,
            string description);

        void UpdateService(
            int id,
            string name,
            string code,
            string description,
            string fulfillmentGroup,
            string requestType,
            int estimatedHours,
            string defaultTaskTypeName,
            string defaultPriority,
            string defaultQueryTypeName,
            string defaultItProjectName,
            string defaultUserQueryTag,
            string defaultPerformerDepartName,
            string defaultShortDescription,
            int slaHours);

        void DeleteNode(int id);
    }
}
