using SDNet.Models;

namespace SDNet.Data
{
    public sealed class DepartmentScopedTaskStoreProxy : ISDTaskStore
    {
        private const int AdministratorRoleId = 1;

        private readonly SqlSDTaskStore _inner;
        private readonly CurrentUserContext _currentUserContext;

        public DepartmentScopedTaskStoreProxy(SqlSDTaskStore inner, CurrentUserContext currentUserContext)
        {
            _inner = inner;
            _currentUserContext = currentUserContext;
        }

        public IReadOnlyList<SDTask> GetAll()
        {
            UserInfo? currentUser = _currentUserContext.CurrentUser;
            if (currentUser is null)
            {
                return [];
            }

            if (IsAdministrator(currentUser))
            {
                return _inner.GetAll();
            }

            return _inner
                .GetAll()
                .Where(task => CanAccessTask(task, currentUser))
                .ToList();
        }

        public async Task<IReadOnlyList<SDTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UserInfo? currentUser = _currentUserContext.CurrentUser;
            if (currentUser is null)
            {
                return [];
            }

            IReadOnlyList<SDTask> tasks = await _inner.GetAllAsync(cancellationToken);
            if (IsAdministrator(currentUser))
            {
                return tasks;
            }

            return tasks
                .Where(task => CanAccessTask(task, currentUser))
                .ToList();
        }

        public SDTask CreateNew(string taskTypeName)
        {
            EnsureAuthorizedUser();

            SDTask task = _inner.CreateNew(taskTypeName);
            NormalizeTaskForCurrentUser(task);
            _inner.Save(task);
            return task;
        }

        public async Task<SDTask> CreateNewAsync(string taskTypeName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureAuthorizedUser();

            SDTask task = await _inner.CreateNewAsync(taskTypeName, cancellationToken);
            NormalizeTaskForCurrentUser(task);
            await _inner.SaveAsync(task, cancellationToken);
            return task;
        }

        public SDTask Clone(Guid id)
        {
            EnsureTaskAccess(id);
            SDTask clone = _inner.Clone(id);
            NormalizeTaskForCurrentUser(clone);
            _inner.Save(clone);
            return clone;
        }

        public async Task<SDTask> CloneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureTaskAccess(id);

            SDTask clone = await _inner.CloneAsync(id, cancellationToken);
            NormalizeTaskForCurrentUser(clone);
            await _inner.SaveAsync(clone, cancellationToken);
            return clone;
        }

        public SDTask? GetById(Guid id)
        {
            SDTask? task = _inner.GetById(id);
            if (task is null)
            {
                return null;
            }

            EnsureTaskAccess(task);
            return task;
        }

        public async Task<SDTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SDTask? task = await _inner.GetByIdAsync(id, cancellationToken);
            if (task is null)
            {
                return null;
            }

            EnsureTaskAccess(task);
            return task;
        }

        public int PeekNextUserQueryId()
        {
            EnsureAuthorizedUser();
            return _inner.PeekNextUserQueryId();
        }

        public Task<int> PeekNextUserQueryIdAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureAuthorizedUser();
            return _inner.PeekNextUserQueryIdAsync(cancellationToken);
        }

        public void Save(SDTask task)
        {
            ArgumentNullException.ThrowIfNull(task);

            UserInfo currentUser = EnsureAuthorizedUser();
            if (!IsAdministrator(currentUser) && task.Id != Guid.Empty)
            {
                SDTask? existingTask = _inner.GetById(task.Id);
                if (existingTask is not null)
                {
                    EnsureTaskAccess(existingTask);
                }
            }

            NormalizeTaskForCurrentUser(task);
            _inner.Save(task);
        }

        public Task SaveAsync(SDTask task, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Save(task);
            return Task.CompletedTask;
        }

        public void Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return;
            }

            EnsureTaskAccess(id);
            _inner.Delete(id);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Delete(id);
            return Task.CompletedTask;
        }

        private UserInfo EnsureAuthorizedUser()
        {
            UserInfo? currentUser = _currentUserContext.CurrentUser;
            if (currentUser is null)
            {
                throw new UnauthorizedAccessException("User is not authorized.");
            }

            return currentUser;
        }

        private void EnsureTaskAccess(Guid taskId)
        {
            SDTask? task = _inner.GetById(taskId);
            if (task is null)
            {
                return;
            }

            EnsureTaskAccess(task);
        }

        private void EnsureTaskAccess(SDTask task)
        {
            UserInfo currentUser = EnsureAuthorizedUser();
            if (!CanAccessTask(task, currentUser))
            {
                throw new UnauthorizedAccessException("Access to another department task is denied.");
            }
        }

        private void NormalizeTaskForCurrentUser(SDTask task)
        {
            UserInfo currentUser = EnsureAuthorizedUser();
            if (IsAdministrator(currentUser))
            {
                return;
            }

            task.UserDepartName = currentUser.UserDepartName;
        }

        private static bool CanAccessTask(SDTask task, UserInfo user)
        {
            return IsAdministrator(user) ||
                   string.Equals(task.UserDepartName, user.UserDepartName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAdministrator(UserInfo user)
        {
            return user.UserRoleId == AdministratorRoleId ||
                   string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase);
        }
    }
}
