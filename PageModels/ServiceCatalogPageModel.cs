using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Models;
using SDNet.Models.ServiceCatalog;
using SDNet.Services.ServiceCatalog;

namespace SDNet.PageModels
{
    public partial class ServiceCatalogPageModel : ObservableObject
    {
        private readonly CurrentUserContext _currentUserContext;
        private readonly IServiceCatalogDataService _serviceCatalogDataService;
        private readonly IServiceCatalogAdminService _serviceCatalogAdminService;
        private readonly HashSet<int> _expandedCategoryIds = [];

        public ObservableCollection<ServiceCatalogTreeItem> VisibleNodes { get; } = [];
        public ObservableCollection<ServiceCatalogCategoryOption> ParentCategories { get; } = [];

        [ObservableProperty]
        private ServiceCatalogTreeItem? _selectedNode;

        [ObservableProperty]
        private ServiceCatalogCategoryOption? _selectedParentCategory;

        [ObservableProperty]
        private string _newCategoryName = string.Empty;

        [ObservableProperty]
        private string _newCategoryCode = string.Empty;

        [ObservableProperty]
        private string _newCategoryDescription = string.Empty;

        [ObservableProperty]
        private string _newServiceName = string.Empty;

        [ObservableProperty]
        private string _newServiceCode = string.Empty;

        [ObservableProperty]
        private string _newServiceDescription = string.Empty;

        [ObservableProperty]
        private string _newServiceFulfillmentGroup = string.Empty;

        [ObservableProperty]
        private string _newServiceRequestType = "Request";

        [ObservableProperty]
        private string _newServiceEstimatedHours = "8";

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private int _totalServicesCount;

        [ObservableProperty]
        private string _editNodeName = string.Empty;

        [ObservableProperty]
        private string _editNodeCode = string.Empty;

        [ObservableProperty]
        private string _editNodeDescription = string.Empty;

        [ObservableProperty]
        private string _editServiceFulfillmentGroup = string.Empty;

        [ObservableProperty]
        private string _editServiceRequestType = string.Empty;

        [ObservableProperty]
        private string _editServiceEstimatedHours = "0";

        public bool IsAdministrator => IsAdministratorUser(_currentUserContext.CurrentUser);

        public bool HasSelectedNode => SelectedNode is not null;

        public bool NoSelectedNode => !HasSelectedNode;

        public bool IsSelectedCategory => SelectedNode?.IsCategory == true;

        public bool IsSelectedService => SelectedNode is not null && !SelectedNode.IsCategory;

        public ServiceCatalogPageModel(
            CurrentUserContext currentUserContext,
            IServiceCatalogDataService serviceCatalogDataService,
            IServiceCatalogAdminService serviceCatalogAdminService)
        {
            _currentUserContext = currentUserContext;
            _serviceCatalogDataService = serviceCatalogDataService;
            _serviceCatalogAdminService = serviceCatalogAdminService;
        }

        partial void OnSelectedNodeChanged(ServiceCatalogTreeItem? value)
        {
            OnPropertyChanged(nameof(HasSelectedNode));
            OnPropertyChanged(nameof(NoSelectedNode));
            OnPropertyChanged(nameof(IsSelectedCategory));
            OnPropertyChanged(nameof(IsSelectedService));

            if (value is null)
            {
                ClearEditFields();
                return;
            }

            PopulateEditFields(value);

            if (!IsAdministrator)
            {
                return;
            }

            int? parentCategoryId = value.IsCategory
                ? value.Id
                : value.Component.Parent?.Id;

            ServiceCatalogCategoryOption? matchingOption = ParentCategories
                .FirstOrDefault(option => option.Id == parentCategoryId);

            if (matchingOption is not null)
            {
                SelectedParentCategory = matchingOption;
            }
        }

        [RelayCommand]
        private void Appearing()
        {
            LoadCatalogTree();
        }

        [RelayCommand]
        private void Reload()
        {
            _serviceCatalogDataService.InvalidateCache();
            LoadCatalogTree();
        }

        [RelayCommand]
        private void ToggleNode(ServiceCatalogTreeItem item)
        {
            if (item is null || !item.IsCategory)
            {
                return;
            }

            if (_expandedCategoryIds.Contains(item.Id))
            {
                _expandedCategoryIds.Remove(item.Id);
            }
            else
            {
                _expandedCategoryIds.Add(item.Id);
            }

            BuildVisibleTree();
        }

        [RelayCommand]
        private async Task AddCategory()
        {
            if (!IsAdministrator)
            {
                await AppShell.DisplaySnackbarAsync("Добавлять категории каталога может только администратор.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                StatusMessage = "Введите название категории.";
                return;
            }

            string code = string.IsNullOrWhiteSpace(NewCategoryCode)
                ? $"CAT_{DateTime.Now:yyyyMMddHHmmss}"
                : NewCategoryCode.Trim();

            try
            {
                ServiceCatalogCategory created = _serviceCatalogAdminService.AddCategory(
                    NewCategoryName,
                    code,
                    NewCategoryDescription,
                    SelectedParentCategory?.Id);

                _expandedCategoryIds.Add(created.Id);
                if (created.Parent is not null)
                {
                    _expandedCategoryIds.Add(created.Parent.Id);
                }

                _serviceCatalogDataService.InvalidateCache();
                LoadCatalogTree();

                NewCategoryName = string.Empty;
                NewCategoryCode = string.Empty;
                NewCategoryDescription = string.Empty;
                StatusMessage = "Категория каталога услуг сохранена.";
                await AppShell.DisplayToastAsync("Категория добавлена");
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task AddService()
        {
            if (!IsAdministrator)
            {
                await AppShell.DisplaySnackbarAsync("Добавлять услуги каталога может только администратор.");
                return;
            }

            if (SelectedParentCategory?.Id is null)
            {
                StatusMessage = "Для конечной услуги нужно выбрать родительскую категорию.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewServiceName))
            {
                StatusMessage = "Введите название услуги.";
                return;
            }

            string code = string.IsNullOrWhiteSpace(NewServiceCode)
                ? $"SVC_{DateTime.Now:yyyyMMddHHmmss}"
                : NewServiceCode.Trim();

            int estimatedHours = int.TryParse(NewServiceEstimatedHours, out int parsedHours) && parsedHours >= 0
                ? parsedHours
                : 0;

            try
            {
                _serviceCatalogAdminService.AddService(
                    SelectedParentCategory.Id.Value,
                    NewServiceName,
                    code,
                    NewServiceDescription,
                    NewServiceFulfillmentGroup,
                    NewServiceRequestType,
                    estimatedHours);

                _expandedCategoryIds.Add(SelectedParentCategory.Id.Value);
                _serviceCatalogDataService.InvalidateCache();
                LoadCatalogTree();

                NewServiceName = string.Empty;
                NewServiceCode = string.Empty;
                NewServiceDescription = string.Empty;
                NewServiceFulfillmentGroup = string.Empty;
                NewServiceRequestType = "Request";
                NewServiceEstimatedHours = "8";
                StatusMessage = "Конечная услуга каталога сохранена.";
                await AppShell.DisplayToastAsync("Услуга добавлена");
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task SaveSelectedNode()
        {
            if (!IsAdministrator)
            {
                await AppShell.DisplaySnackbarAsync("Редактирование каталога доступно только администратору.");
                return;
            }

            if (SelectedNode is null)
            {
                StatusMessage = "Сначала выберите узел каталога.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditNodeName))
            {
                StatusMessage = "Название узла обязательно.";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditNodeCode))
            {
                StatusMessage = "Код узла обязателен.";
                return;
            }

            try
            {
                if (SelectedNode.IsCategory)
                {
                    _serviceCatalogAdminService.UpdateCategory(
                        SelectedNode.Id,
                        EditNodeName,
                        EditNodeCode,
                        EditNodeDescription);
                }
                else
                {
                    int estimatedHours = int.TryParse(EditServiceEstimatedHours, out int parsedHours) && parsedHours >= 0
                        ? parsedHours
                        : 0;

                    _serviceCatalogAdminService.UpdateService(
                        SelectedNode.Id,
                        EditNodeName,
                        EditNodeCode,
                        EditNodeDescription,
                        EditServiceFulfillmentGroup,
                        EditServiceRequestType,
                        estimatedHours);
                }

                int selectedId = SelectedNode.Id;
                _serviceCatalogDataService.InvalidateCache();
                LoadCatalogTree();
                ReselectNode(selectedId);
                StatusMessage = "Изменения каталога сохранены.";
                await AppShell.DisplayToastAsync("Узел обновлен");
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        [RelayCommand]
        private async Task DeleteSelectedNode()
        {
            if (!IsAdministrator)
            {
                await AppShell.DisplaySnackbarAsync("Удаление узлов каталога доступно только администратору.");
                return;
            }

            if (SelectedNode is null)
            {
                StatusMessage = "Сначала выберите узел каталога.";
                return;
            }

            try
            {
                _serviceCatalogAdminService.DeleteNode(SelectedNode.Id);
                if (SelectedNode.IsCategory)
                {
                    _expandedCategoryIds.Remove(SelectedNode.Id);
                }

                _serviceCatalogDataService.InvalidateCache();
                SelectedNode = null;
                LoadCatalogTree();
                StatusMessage = "Узел каталога удален.";
                await AppShell.DisplayToastAsync("Узел удален");
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void LoadCatalogTree()
        {
            BuildParentCategoryOptions();
            BuildVisibleTree();
        }

        private void BuildParentCategoryOptions()
        {
            int? previousParentId = SelectedParentCategory?.Id;

            ParentCategories.Clear();
            ParentCategories.Add(new ServiceCatalogCategoryOption
            {
                Id = null,
                DisplayName = "Корень каталога"
            });

            ServiceCatalogCategory root = _serviceCatalogDataService.GetCatalog();
            foreach ((ServiceCatalogCategory category, int level) in EnumerateCategories(root, 0))
            {
                ParentCategories.Add(new ServiceCatalogCategoryOption
                {
                    Id = category.Id,
                    DisplayName = $"{new string(' ', level * 2)}{category.Name}"
                });
            }

            SelectedParentCategory = ParentCategories.FirstOrDefault(option => option.Id == previousParentId)
                                     ?? ParentCategories.FirstOrDefault();
        }

        private void BuildVisibleTree()
        {
            VisibleNodes.Clear();
            ServiceCatalogCategory root = _serviceCatalogDataService.GetCatalog();

            if (_expandedCategoryIds.Count == 0)
            {
                foreach (ServiceCatalogCategory category in root.Children.OfType<ServiceCatalogCategory>())
                {
                    _expandedCategoryIds.Add(category.Id);
                }
            }

            foreach (ServiceCatalogComponent child in root.Children)
            {
                AddVisibleNode(child, 0);
            }

            TotalServicesCount = root.CountServices();
        }

        private void ReselectNode(int nodeId)
        {
            SelectedNode = VisibleNodes.FirstOrDefault(node => node.Id == nodeId);
        }

        private void AddVisibleNode(ServiceCatalogComponent component, int level)
        {
            bool isExpanded = component is ServiceCatalogCategory category &&
                              _expandedCategoryIds.Contains(category.Id);

            VisibleNodes.Add(new ServiceCatalogTreeItem(component, level, isExpanded));

            if (component is not ServiceCatalogCategory composite || !isExpanded)
            {
                return;
            }

            foreach (ServiceCatalogComponent child in composite.Children)
            {
                AddVisibleNode(child, level + 1);
            }
        }

        private static IEnumerable<(ServiceCatalogCategory Category, int Level)> EnumerateCategories(
            ServiceCatalogCategory root,
            int startLevel)
        {
            foreach (ServiceCatalogCategory category in root.Children.OfType<ServiceCatalogCategory>())
            {
                yield return (category, startLevel);

                foreach ((ServiceCatalogCategory nested, int nestedLevel) in EnumerateCategories(category, startLevel + 1))
                {
                    yield return (nested, nestedLevel);
                }
            }
        }

        private void PopulateEditFields(ServiceCatalogTreeItem item)
        {
            EditNodeName = item.Name;
            EditNodeCode = item.Code;
            EditNodeDescription = item.Description;

            if (item.Component is ServiceCatalogServiceItem service)
            {
                EditServiceFulfillmentGroup = service.FulfillmentGroup;
                EditServiceRequestType = service.RequestType;
                EditServiceEstimatedHours = service.EstimatedHours.ToString();
            }
            else
            {
                EditServiceFulfillmentGroup = string.Empty;
                EditServiceRequestType = string.Empty;
                EditServiceEstimatedHours = "0";
            }
        }

        private void ClearEditFields()
        {
            EditNodeName = string.Empty;
            EditNodeCode = string.Empty;
            EditNodeDescription = string.Empty;
            EditServiceFulfillmentGroup = string.Empty;
            EditServiceRequestType = string.Empty;
            EditServiceEstimatedHours = "0";
        }

        private static bool IsAdministratorUser(UserInfo? user)
        {
            return user is not null &&
                   (user.UserRoleId == 1 ||
                    string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase));
        }
    }
}
