using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Models;
using SDNet.Services;

namespace SDNet.PageModels
{
    public partial class ManageReferencesPageModel : ObservableObject
    {
        private const string DepartmentsCatalog = "Департаменты";
        private const string QueryTypesCatalog = "Типы запросов";
        private const string ItProjectsCatalog = "IT-проекты";

        private readonly CurrentUserContext _currentUserContext;
        private readonly IReferenceCatalogAdminService _referenceCatalogAdminService;
        private readonly ITaskReferenceDataService _taskReferenceDataService;

        public ObservableCollection<ReferenceValue> Values { get; } = [];

        public IReadOnlyList<string> Catalogs { get; } =
        [
            DepartmentsCatalog,
            QueryTypesCatalog,
            ItProjectsCatalog
        ];

        [ObservableProperty]
        private string _selectedCatalog = DepartmentsCatalog;

        [ObservableProperty]
        private ReferenceValue? _selectedValue;

        [ObservableProperty]
        private string _newName = string.Empty;

        [ObservableProperty]
        private string _newCode = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public ManageReferencesPageModel(
            CurrentUserContext currentUserContext,
            IReferenceCatalogAdminService referenceCatalogAdminService,
            ITaskReferenceDataService taskReferenceDataService)
        {
            _currentUserContext = currentUserContext;
            _referenceCatalogAdminService = referenceCatalogAdminService;
            _taskReferenceDataService = taskReferenceDataService;
        }

        partial void OnSelectedCatalogChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            LoadValues();
        }

        [RelayCommand]
        private async Task Appearing()
        {
            if (!IsAdministrator(_currentUserContext.CurrentUser))
            {
                await AppShell.DisplaySnackbarAsync("Доступ к справочникам есть только у администратора.");
                await Shell.Current.GoToAsync("//task-list");
                return;
            }

            LoadValues();
        }

        [RelayCommand]
        private void Reload()
        {
            LoadValues();
        }

        [RelayCommand]
        private async Task AddValue()
        {
            if (!IsAdministrator(_currentUserContext.CurrentUser))
            {
                await AppShell.DisplaySnackbarAsync("Недостаточно прав.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewName))
            {
                StatusMessage = "Введите название.";
                return;
            }

            try
            {
                ReferenceValue saved = SelectedCatalog switch
                {
                    DepartmentsCatalog => _referenceCatalogAdminService.AddDepartment(NewName, NewCode),
                    QueryTypesCatalog => _referenceCatalogAdminService.AddQueryType(NewName, NewCode),
                    ItProjectsCatalog => _referenceCatalogAdminService.AddItProject(NewName, NewCode),
                    _ => throw new InvalidOperationException("Неизвестный тип справочника.")
                };

                _taskReferenceDataService.InvalidateCache();
                LoadValues();
                SelectedValue = Values.FirstOrDefault(v => v.Id == saved.Id);
                StatusMessage = "Значение сохранено.";
                NewName = string.Empty;
                NewCode = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task DeleteSelected()
        {
            if (!IsAdministrator(_currentUserContext.CurrentUser))
            {
                await AppShell.DisplaySnackbarAsync("Недостаточно прав.");
                return;
            }

            if (SelectedValue is null)
            {
                StatusMessage = "Выберите значение для удаления.";
                return;
            }

            try
            {
                switch (SelectedCatalog)
                {
                    case DepartmentsCatalog:
                        _referenceCatalogAdminService.DeleteDepartment(SelectedValue.Id);
                        break;
                    case QueryTypesCatalog:
                        _referenceCatalogAdminService.DeleteQueryType(SelectedValue.Id);
                        break;
                    case ItProjectsCatalog:
                        _referenceCatalogAdminService.DeleteItProject(SelectedValue.Id);
                        break;
                    default:
                        throw new InvalidOperationException("Неизвестный тип справочника.");
                }

                _taskReferenceDataService.InvalidateCache();
                StatusMessage = "Значение удалено.";
                LoadValues();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void LoadValues()
        {
            Values.Clear();

            IReadOnlyList<ReferenceValue> values = SelectedCatalog switch
            {
                DepartmentsCatalog => _referenceCatalogAdminService.GetDepartments(),
                QueryTypesCatalog => _referenceCatalogAdminService.GetQueryTypes(),
                ItProjectsCatalog => _referenceCatalogAdminService.GetItProjects(),
                _ => []
            };

            foreach (ReferenceValue value in values.OrderBy(v => v.Name))
            {
                Values.Add(value);
            }

            SelectedValue = null;
        }

        private static bool IsAdministrator(UserInfo? user)
        {
            return user is not null &&
                   (user.UserRoleId == 1 ||
                    string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase));
        }
    }
}
