using SDNet.Models.ServiceCatalog;

namespace SDNet.Services.ServiceCatalog
{
    public interface IServiceCatalogDataService
    {
        ServiceCatalogCategory GetCatalog();

        void InvalidateCache();
    }
}
