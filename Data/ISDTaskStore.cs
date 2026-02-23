using SDNet.Models;

namespace SDNet.Data
{
    public interface ISDTaskStore
    {
        IReadOnlyList<SDTask> GetAll();
        Task<IReadOnlyList<SDTask>> GetAllAsync(CancellationToken cancellationToken = default);
        SDTask CreateNew(string taskTypeName);
        Task<SDTask> CreateNewAsync(string taskTypeName, CancellationToken cancellationToken = default);
        SDTask Clone(Guid id);
        Task<SDTask> CloneAsync(Guid id, CancellationToken cancellationToken = default);
        SDTask? GetById(Guid id);
        Task<SDTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        int PeekNextUserQueryId();
        Task<int> PeekNextUserQueryIdAsync(CancellationToken cancellationToken = default);
        void Save(SDTask task);
        Task SaveAsync(SDTask task, CancellationToken cancellationToken = default);
        void Delete(Guid id);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
