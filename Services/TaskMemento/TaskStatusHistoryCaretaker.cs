using System.Text.Json;
using SDNet.Models;

namespace SDNet.Services.TaskMemento
{
    public interface ITaskStatusHistoryCaretaker
    {
        string StoragePath { get; }
        void Push(TaskStatusMemento memento);
        TaskStatusMemento? PopLast(Guid taskId);
        Task PushAsync(TaskStatusMemento memento, CancellationToken cancellationToken = default);
        Task<TaskStatusMemento?> PopLastAsync(Guid taskId, CancellationToken cancellationToken = default);
    }

    public sealed class TaskStatusHistoryCaretaker : ITaskStatusHistoryCaretaker
    {
        private readonly SemaphoreSlim _sync = new(1, 1);
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public TaskStatusHistoryCaretaker()
        {
            StoragePath = Path.Combine(FileSystem.AppDataDirectory, "task-status-history.json");
        }

        public string StoragePath { get; }

        public void Push(TaskStatusMemento memento)
        {
            ArgumentNullException.ThrowIfNull(memento);

            _sync.Wait();
            try
            {
                List<TaskStatusMemento> items = Load();
                items.Add(memento);
                Save(items);
            }
            finally
            {
                _sync.Release();
            }
        }

        public TaskStatusMemento? PopLast(Guid taskId)
        {
            _sync.Wait();
            try
            {
                List<TaskStatusMemento> items = Load();
                TaskStatusMemento? memento = items
                    .Where(item => item.TaskId == taskId)
                    .OrderByDescending(item => item.CapturedAt)
                    .FirstOrDefault();

                if (memento is null)
                {
                    return null;
                }

                items.Remove(memento);
                Save(items);
                return memento;
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task PushAsync(TaskStatusMemento memento, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(memento);

            await _sync.WaitAsync(cancellationToken);
            try
            {
                List<TaskStatusMemento> items = await LoadAsync(cancellationToken);
                items.Add(memento);
                await SaveAsync(items, cancellationToken);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task<TaskStatusMemento?> PopLastAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            await _sync.WaitAsync(cancellationToken);
            try
            {
                List<TaskStatusMemento> items = await LoadAsync(cancellationToken);
                TaskStatusMemento? memento = items
                    .Where(item => item.TaskId == taskId)
                    .OrderByDescending(item => item.CapturedAt)
                    .FirstOrDefault();

                if (memento is null)
                {
                    return null;
                }

                items.Remove(memento);
                await SaveAsync(items, cancellationToken);
                return memento;
            }
            finally
            {
                _sync.Release();
            }
        }

        private async Task<List<TaskStatusMemento>> LoadAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(StoragePath))
            {
                return [];
            }

            await using FileStream stream = File.OpenRead(StoragePath);
            return await JsonSerializer.DeserializeAsync<List<TaskStatusMemento>>(stream, _serializerOptions, cancellationToken) ?? [];
        }

        private async Task SaveAsync(List<TaskStatusMemento> items, CancellationToken cancellationToken)
        {
            string? directory = Path.GetDirectoryName(StoragePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream stream = File.Create(StoragePath);
            await JsonSerializer.SerializeAsync(stream, items, _serializerOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        private List<TaskStatusMemento> Load()
        {
            if (!File.Exists(StoragePath))
            {
                return [];
            }

            using FileStream stream = File.OpenRead(StoragePath);
            return JsonSerializer.Deserialize<List<TaskStatusMemento>>(stream, _serializerOptions) ?? [];
        }

        private void Save(List<TaskStatusMemento> items)
        {
            string? directory = Path.GetDirectoryName(StoragePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using FileStream stream = File.Create(StoragePath);
            JsonSerializer.Serialize(stream, items, _serializerOptions);
            stream.Flush();
        }
    }
}
