using SDNet.Models;

namespace SDNet.Services
{
    public interface IUserSettingsService
    {
        string SettingsFilePath { get; }

        UserSettings Current { get; }

        Task<UserSettings> LoadAsync(CancellationToken cancellationToken = default);

        Task SaveAsync(UserSettings settings, CancellationToken cancellationToken = default);
    }
}
