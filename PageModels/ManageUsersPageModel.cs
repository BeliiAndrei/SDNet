using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Models;
using SDNet.Services;
using SDNet.Services.Auth;

namespace SDNet.PageModels
{
    public partial class ManageUsersPageModel : ObservableObject
    {
        private static readonly UserInfoDirector UserInfoDirector = new();
        private readonly IUserDirectoryService _userDirectoryService;
        private readonly CurrentUserContext _currentUserContext;
        private readonly ITaskReferenceDataService _taskReferenceDataService;

        public ObservableCollection<UserInfo> Users { get; } = [];
        public ObservableCollection<string> Departments { get; } = [];
        public IReadOnlyList<string> Roles { get; } = ["Administrator", "User"];

        [ObservableProperty]
        private UserInfo? _selectedUser;

        [ObservableProperty]
        private int _editUserId;

        [ObservableProperty]
        private string _login = string.Empty;

        [ObservableProperty]
        private string _userFullName = string.Empty;

        [ObservableProperty]
        private string _selectedRole = "User";

        [ObservableProperty]
        private string _selectedDepartment = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private bool _isActive = true;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public ManageUsersPageModel(
            IUserDirectoryService userDirectoryService,
            CurrentUserContext currentUserContext,
            ITaskReferenceDataService taskReferenceDataService)
        {
            _userDirectoryService = userDirectoryService;
            _currentUserContext = currentUserContext;
            _taskReferenceDataService = taskReferenceDataService;
        }

        partial void OnSelectedUserChanged(UserInfo? value)
        {
            if (value is null)
            {
                return;
            }

            EditUserId = value.UserId;
            Login = value.UserName;
            UserFullName = value.UserFullName;
            SelectedRole = value.UserRoleName;
            SelectedDepartment = value.UserDepartName;
            Email = value.Email;
            PhoneNumber = value.PhoneNumber;
            IsActive = value.IsActive;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            if (!IsAdministrator(_currentUserContext.CurrentUser))
            {
                await AppShell.DisplaySnackbarAsync("Доступ к управлению пользователями есть только у администратора.");
                await Shell.Current.GoToAsync("//task-list");
                return;
            }

            LoadDepartments();
            ReloadUsers();

            if (EditUserId == 0)
            {
                NewUser();
            }
        }

        [RelayCommand]
        private void NewUser()
        {
            EditUserId = 0;
            Login = string.Empty;
            UserFullName = string.Empty;
            SelectedRole = "User";
            SelectedDepartment = Departments.FirstOrDefault() ?? string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            IsActive = true;
            SelectedUser = null;
            StatusMessage = "Новый пользователь.";
        }

        [RelayCommand]
        private async Task SaveUser()
        {
            if (!IsAdministrator(_currentUserContext.CurrentUser))
            {
                await AppShell.DisplaySnackbarAsync("Недостаточно прав.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(UserFullName))
            {
                StatusMessage = "Логин и ФИО обязательны.";
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedDepartment))
            {
                StatusMessage = "Выберите департамент.";
                return;
            }

            UserInfo? existing = _userDirectoryService.GetByLogin(Login);
            if (existing is not null && existing.UserId != EditUserId)
            {
                StatusMessage = "Пользователь с таким логином уже существует.";
                return;
            }

            var data = new UserInfoBuildData
            {
                UserId = EditUserId,
                UserName = Login,
                UserFullName = UserFullName,
                UserRoleName = SelectedRole,
                UserDepartName = SelectedDepartment,
                Email = Email,
                PhoneNumber = PhoneNumber,
                IsActive = IsActive
            };

            UserInfo user = UserInfoDirector.BuildForSave(new UserInfoBuilder(), data);

            UserInfo saved = _userDirectoryService.Save(user);
            ReloadUsers();
            SelectedUser = Users.FirstOrDefault(u => u.UserId == saved.UserId);
            StatusMessage = "Изменения сохранены.";
            await AppShell.DisplayToastAsync("Пользователь сохранен");
        }

        private void LoadDepartments()
        {
            Departments.Clear();
            IReadOnlyList<string> values = _taskReferenceDataService.GetDepartments();
            foreach (string name in values)
            {
                Departments.Add(name);
            }

            if (!Departments.Any(d => string.Equals(d, SelectedDepartment, StringComparison.OrdinalIgnoreCase)))
            {
                SelectedDepartment = Departments.FirstOrDefault() ?? string.Empty;
            }
        }

        private void ReloadUsers()
        {
            Users.Clear();
            foreach (UserInfo user in _userDirectoryService.GetAllUsers())
            {
                Users.Add(user);
            }
        }

        private static bool IsAdministrator(UserInfo? user)
        {
            return user is not null &&
                   (user.UserRoleId == 1 || string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase));
        }
    }
}
