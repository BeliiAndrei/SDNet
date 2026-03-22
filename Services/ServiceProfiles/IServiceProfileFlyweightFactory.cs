using SDNet.Models.ServiceProfiles;

namespace SDNet.Services.ServiceProfiles
{
    public interface IServiceProfileFlyweightFactory
    {
        IReadOnlyList<IServiceProfileFlyweight> GetAll();

        IServiceProfileFlyweight? GetById(int? id);

        IServiceProfileFlyweight? GetByServiceCatalogNodeId(int serviceCatalogNodeId);

        void InvalidateCache();
    }
}
