using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Models;
using SDNet.Services.Navigation;

namespace SDNet.PageModels
{
    public partial class LoginPageModel : ObservableObject
    {
        private readonly CurrentUserContext _currentUserContext;
        private readonly IAppNavigationService _appNavigationService;

        [ObservableProperty]
        private string _login = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _sqlServer = "localhost";

        [ObservableProperty]
        private string _sqlDatabase = "SDNet";

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginPageModel(CurrentUserContext currentUserContext, IAppNavigationService appNavigationService)
        {
            _currentUserContext = currentUserContext;
            _appNavigationService = appNavigationService;
        }

        [RelayCommand]
        private async Task SignIn()
        {
            if (IsBusy)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                ErrorMessage = "Введите логин.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;
            try
            {
                SqlConnectionContext.Initialize(SqlServer, SqlDatabase, Login);
                UserInfo user = await _currentUserContext.AuthorizeAsync(Login, Password);
                SqlConnectionContext.Initialize(SqlServer, SqlDatabase, user.UserName);
                _appNavigationService.RequestOpenShell();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка авторизации: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
