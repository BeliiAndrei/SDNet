using SDNet.Models;

namespace SDNet.Services
{
    public sealed class CurrentUserContext
    {
        private static CurrentUserContext? _instance;
        private static readonly object _instanceLock = new();

        private readonly SemaphoreSlim _sync = new(1, 1);
        private IAuthorizationService _authorizationService;

        private CurrentUserContext()
        {
            _authorizationService = new UnconfiguredAuthorizationService();
        }

        public static CurrentUserContext Instance => GetInstance();

        public static CurrentUserContext GetInstance()
        {
            if (_instance is not null)
            {
                return _instance;
            }

            lock (_instanceLock)
            {
                _instance ??= new CurrentUserContext();
                return _instance;
            }
        }

        public static void Initialize(IAuthorizationService authorizationService)
        {
            CurrentUserContext instance = GetInstance();
            instance.ConfigureAuthorizationService(authorizationService);
        }

        public UserInfo? CurrentUser { get; private set; }

        public bool IsAuthorized => CurrentUser is not null;

        public void ConfigureAuthorizationService(IAuthorizationService authorizationService)
        {
            ArgumentNullException.ThrowIfNull(authorizationService);
            _authorizationService = authorizationService;
        }

        public async Task<UserInfo> AuthorizeAsync(string login, string password, CancellationToken cancellationToken = default)
        {
            await _sync.WaitAsync(cancellationToken);
            try
            {
                UserInfo userInfo = await _authorizationService.AuthorizeAsync(login, password, cancellationToken);
                userInfo.LastActivityAt = DateTime.Now;
                CurrentUser = userInfo;
                return userInfo;
            }
            finally
            {
                _sync.Release();
            }
        }

        public void TouchActivity()
        {
            if (CurrentUser is null)
            {
                return;
            }

            CurrentUser.LastActivityAt = DateTime.Now;
        }

        public void SignOut()
        {
            CurrentUser = null;
        }

        private sealed class UnconfiguredAuthorizationService : IAuthorizationService
        {
            public Task<UserInfo> AuthorizeAsync(string login, string password, CancellationToken cancellationToken = default)
            {
                throw new InvalidOperationException("Сервис авторизации не настроен.");
            }
        }
    }
}
