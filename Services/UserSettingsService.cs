using System.Text.Json;
using System.Text.Json.Serialization;
using SDNet.Models;

namespace SDNet.Services
{
    public sealed class UserSettingsService : IUserSettingsService, IDisposable
    {
        private readonly SemaphoreSlim _sync = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private UserSettings? _current;
        private bool _isDisposed;

        public UserSettingsService()
        {
            SettingsFilePath = Path.Combine(FileSystem.AppDataDirectory, "user-settings.json");
        }

        public string SettingsFilePath { get; }

        public UserSettings Current => _current is null ? new UserSettings() : Copy(_current);

        public async Task<UserSettings> LoadAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await _sync.WaitAsync(cancellationToken);
            try
            {
                if (_current is not null)
                {
                    return Copy(_current);
                }

                _current = await LoadCoreAsync(cancellationToken);
                return Copy(_current);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task SaveAsync(UserSettings settings, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ThrowIfDisposed();

            await _sync.WaitAsync(cancellationToken);
            try
            {
                _current = Copy(settings);
                await SaveCoreAsync(_current, cancellationToken);
            }
            finally
            {
                _sync.Release();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _sync.Dispose();
            _isDisposed = true;
        }

        private async Task<UserSettings> LoadCoreAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(SettingsFilePath))
            {
                var defaults = new UserSettings();
                await SaveCoreAsync(defaults, cancellationToken);
                return defaults;
            }

            try
            {
                await using FileStream stream = File.OpenRead(SettingsFilePath);
                UserSettings? settings = await JsonSerializer.DeserializeAsync<UserSettings>(stream, _jsonOptions, cancellationToken);
                return settings ?? new UserSettings();
            }
            catch
            {
                var defaults = new UserSettings();
                await SaveCoreAsync(defaults, cancellationToken);
                return defaults;
            }
        }

        private async Task SaveCoreAsync(UserSettings settings, CancellationToken cancellationToken)
        {
            string? directory = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream stream = File.Create(SettingsFilePath);
            await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
        }

        private static UserSettings Copy(UserSettings source)
        {
            return new UserSettings
            {
                SaveTableLayout = source.SaveTableLayout,
                Theme = source.Theme,
                CompactTaskRows = source.CompactTaskRows,
                ConfirmBeforeDelete = source.ConfirmBeforeDelete,
                EnableNotifications = source.EnableNotifications
            };
        }
    }
}
