using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SDNet.Data;
using SDNet.Models;
using SDNet.Models.ServiceProfiles;
using SDNet.Services;
using SDNet.Services.Auth;
using SDNet.Services.ServiceProfiles;
using SDNet.Services.TaskCreation;

namespace SDNet.PageModels
{
    public partial class TaskEditorPageModel : ObservableObject, IQueryAttributable
    {
        private const int AdministratorRoleId = 1;

        private readonly ISDTaskStore _taskStore;
        private readonly CurrentUserContext _currentUserContext;
        private readonly IUserDirectoryService _userDirectoryService;
        private readonly ITaskReferenceDataService _taskReferenceDataService;
        private readonly ISDTaskFactoryMethodService _taskFactoryMethodService;
        private readonly IServiceProfileFlyweightFactory _serviceProfileFlyweightFactory;
        private Guid _taskId;
        private bool _isSyncingServiceProfileSelection;

        public IReadOnlyList<string> TaskTypes => _taskFactoryMethodService.SupportedTaskTypes;
        public IReadOnlyList<string> Priorities { get; } = ["Низкий", "Средний", "Высокий", "Критичный"];
        public IReadOnlyList<string> States { get; } = ["Новая", "В работе", "Согласование", "Закрыта"];
        public ObservableCollection<string> PerformerOptions { get; } = [];
        public ObservableCollection<string> DepartmentOptions { get; } = [];
        public ObservableCollection<string> QueryTypeOptions { get; } = [];
        public ObservableCollection<string> ItProjectOptions { get; } = [];
        public ObservableCollection<ServiceProfileOption> ServiceProfileOptions { get; } = [];

        [ObservableProperty]
        private bool _isExistingTask;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsITTask))]
        [NotifyPropertyChangedFor(nameof(IsHardwareTask))]
        [NotifyPropertyChangedFor(nameof(IsCommunicationTask))]
        [NotifyPropertyChangedFor(nameof(IsAccessTask))]
        [NotifyPropertyChangedFor(nameof(IsSecurityTask))]
        [NotifyPropertyChangedFor(nameof(IsIntegrationTask))]
        private string _selectedTaskType = SDTaskTypes.ITTask;

        [ObservableProperty]
        private int _userQueryId;

        [ObservableProperty]
        private DateTime _dateReg = DateTime.Now;

        [ObservableProperty]
        private string _priority = "Средний";

        [ObservableProperty]
        private string _userFio = string.Empty;

        [ObservableProperty]
        private string _userDepartName = string.Empty;

        [ObservableProperty]
        private string _userQueryTag = string.Empty;

        [ObservableProperty]
        private string _queryTypeName = string.Empty;

        [ObservableProperty]
        private string _itProjectName = string.Empty;

        [ObservableProperty]
        private string _shortDescription = string.Empty;

        [ObservableProperty]
        private ServiceProfileOption? _selectedServiceProfile;

        [ObservableProperty]
        private string _stateName = "Новая";

        [ObservableProperty]
        private DateTime _dateNeedClose = DateTime.Today.AddDays(2);

        [ObservableProperty]
        private string _performerName = string.Empty;

        [ObservableProperty]
        private string _selectedPerformer = string.Empty;

        [ObservableProperty]
        private string _performerDepartName = string.Empty;

        [ObservableProperty]
        private double _performPercent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDateClosedVisible))]
        private DateTime _dateClosed = DateTime.Now;

        [ObservableProperty]
        private string _itSystemArea = string.Empty;

        [ObservableProperty]
        private bool _itRequiresDeployment;

        [ObservableProperty]
        private string _hardwareModel = string.Empty;

        [ObservableProperty]
        private string _hardwareAssetNumber = string.Empty;

        [ObservableProperty]
        private string _communicationChannel = string.Empty;

        [ObservableProperty]
        private string _communicationContact = string.Empty;

        [ObservableProperty]
        private string _accessRole = string.Empty;

        [ObservableProperty]
        private string _accessResource = string.Empty;

        [ObservableProperty]
        private string _securityRiskLevel = string.Empty;

        [ObservableProperty]
        private bool _securityRequiresAudit;

        [ObservableProperty]
        private string _integrationEndpoint = string.Empty;

        [ObservableProperty]
        private string _integrationSystem = string.Empty;

        public bool IsITTask => SelectedTaskType == SDTaskTypes.ITTask;
        public bool IsHardwareTask => SelectedTaskType == SDTaskTypes.HardwareTask;
        public bool IsCommunicationTask => SelectedTaskType == SDTaskTypes.CommunicationTask;
        public bool IsAccessTask => SelectedTaskType == SDTaskTypes.AccessTask;
        public bool IsSecurityTask => SelectedTaskType == SDTaskTypes.SecurityTask;
        public bool IsIntegrationTask => SelectedTaskType == SDTaskTypes.IntegrationTask;
        public bool IsDateClosedVisible => StateName == "Закрыта";

        public TaskEditorPageModel(
            ISDTaskStore taskStore,
            CurrentUserContext currentUserContext,
            IUserDirectoryService userDirectoryService,
            ITaskReferenceDataService taskReferenceDataService,
            ISDTaskFactoryMethodService taskFactoryMethodService,
            IServiceProfileFlyweightFactory serviceProfileFlyweightFactory)
        {
            _taskStore = taskStore;
            _currentUserContext = currentUserContext;
            _userDirectoryService = userDirectoryService;
            _taskReferenceDataService = taskReferenceDataService;
            _taskFactoryMethodService = taskFactoryMethodService;
            _serviceProfileFlyweightFactory = serviceProfileFlyweightFactory;
            FillDefaults();
        }

        partial void OnStateNameChanged(string value)
        {
            OnPropertyChanged(nameof(IsDateClosedVisible));
            if (value == "Закрыта")
            {
                DateClosed = DateTime.Now;
            }
        }

        partial void OnSelectedPerformerChanged(string value)
        {
            PerformerName = value;

            UserInfo? selectedUser = _userDirectoryService.GetByFullName(value);
            if (selectedUser is not null)
            {
                PerformerDepartName = selectedUser.UserDepartName;
            }
        }

        partial void OnSelectedServiceProfileChanged(ServiceProfileOption? value)
        {
            if (_isSyncingServiceProfileSelection || value?.Id is null)
            {
                return;
            }

            IServiceProfileFlyweight? flyweight = _serviceProfileFlyweightFactory.GetById(value.Id);
            if (flyweight is null)
            {
                return;
            }

            ServiceProfileTaskContext context = CaptureServiceProfileContext();
            flyweight.ApplyTo(context);
            ApplyServiceProfileContext(context);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            bool isNew = query.TryGetValue("isNew", out var isNewValue) &&
                         bool.TryParse(isNewValue?.ToString(), out var parsed) &&
                         parsed;
            int? requestedServiceProfileId = TryParseServiceProfileId(query);

            if (isNew)
            {
                IsExistingTask = false;
                FillDefaults();
                SetSelectedServiceProfile(requestedServiceProfileId, applyProfile: true);
                return;
            }

            if (query.TryGetValue("id", out var idObj) &&
                Guid.TryParse(idObj?.ToString(), out Guid id))
            {
                SDTask? task = _taskStore.GetById(id);
                if (task is null)
                {
                    FillDefaults();
                    IsExistingTask = false;
                    return;
                }

                if (!CanAccessDepartment(task.UserDepartName))
                {
                    AppShell.DisplaySnackbarAsync("Нет доступа к задачам другого департамента.").FireAndForgetSafeAsync();
                    Shell.Current.GoToAsync("..").FireAndForgetSafeAsync();
                    FillDefaults();
                    IsExistingTask = false;
                    return;
                }

                _taskId = task.Id;
                IsExistingTask = true;
                FillFromTask(task);
                return;
            }

            IsExistingTask = false;
            FillDefaults();
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(ShortDescription))
            {
                await AppShell.DisplaySnackbarAsync("Заполните краткое описание задачи.");
                return;
            }

            UserInfo? currentUser = _currentUserContext.CurrentUser;
            if (currentUser is not null && !IsAdministrator(currentUser))
            {
                UserDepartName = currentUser.UserDepartName;
                if (!CanAccessDepartment(UserDepartName))
                {
                    await AppShell.DisplaySnackbarAsync("Можно работать только со своим департаментом.");
                    return;
                }
            }

            UserInfo? selectedPerformerUser = _userDirectoryService.GetByFullName(SelectedPerformer);
            if (currentUser is not null &&
                !IsAdministrator(currentUser) &&
                selectedPerformerUser is not null &&
                IsAdministrator(selectedPerformerUser))
            {
                await AppShell.DisplaySnackbarAsync("Пользователь с ролью User не может назначать задачу администратору.");
                return;
            }

            if (!IsExistingTask && !string.IsNullOrWhiteSpace(currentUser?.UserFullName))
            {
                UserFio = currentUser.UserFullName;
            }

            var task = _taskFactoryMethodService.CreateTask(SelectedTaskType);
            task.Id = _taskId == Guid.Empty ? Guid.NewGuid() : _taskId;
            task.UserQueryId = UserQueryId;
            task.DateReg = DateReg;
            task.Priority = Priority;
            task.UserFio = UserFio;
            task.UserDepartName = UserDepartName;
            task.UserQueryTag = UserQueryTag;
            task.QueryTypeName = QueryTypeName;
            task.ItProjectName = ItProjectName;
            task.ShortDescription = ShortDescription;
            task.StateName = StateName;
            task.DateNeedClose = DateNeedClose;
            task.PerformerName = selectedPerformerUser?.UserFullName ?? PerformerName;
            task.PerformerDepartName = selectedPerformerUser?.UserDepartName ?? PerformerDepartName;
            task.PerformPercent = (int)Math.Round(PerformPercent);
            task.DateClosed = StateName == "Закрыта" ? DateClosed : null;
            task.ServiceProfileId = SelectedServiceProfile?.Id;

            ApplyTypeSpecific(task);
            _taskStore.Save(task);
            _taskId = task.Id;
            IsExistingTask = true;

            await Shell.Current.GoToAsync($"..?refresh=true&focusId={task.Id}");
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (_taskId == Guid.Empty)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            _taskStore.Delete(_taskId);
            await Shell.Current.GoToAsync("..?refresh=true");
        }

        [RelayCommand]
        private Task Cancel()
        {
            return Shell.Current.GoToAsync("..");
        }

        private void FillDefaults()
        {
            _taskId = Guid.Empty;
            UserInfo? currentUser = _currentUserContext.CurrentUser;
            LoadReferenceOptions();
            LoadServiceProfileOptions();
            EnsureOption(DepartmentOptions, currentUser?.UserDepartName);

            SelectedTaskType = SDTaskTypes.ITTask;
            UserQueryId = _taskStore.PeekNextUserQueryId();
            DateReg = DateTime.Now;
            Priority = "Средний";
            UserFio = currentUser?.UserFullName ?? string.Empty;
            UserDepartName = ChooseOption(DepartmentOptions, currentUser?.UserDepartName);
            UserQueryTag = "NEW";
            QueryTypeName = ChooseOption(QueryTypeOptions, "Запрос на обслуживание");
            ItProjectName = ChooseOption(ItProjectOptions, "SDNet");
            ShortDescription = string.Empty;
            StateName = "Новая";
            DateNeedClose = DateTime.Today.AddDays(2);
            PerformerName = string.Empty;
            PerformerDepartName = currentUser?.UserDepartName ?? "Service Desk";
            PerformPercent = 0;
            DateClosed = DateTime.Now;
            ItSystemArea = string.Empty;
            ItRequiresDeployment = false;
            HardwareModel = string.Empty;
            HardwareAssetNumber = string.Empty;
            CommunicationChannel = string.Empty;
            CommunicationContact = string.Empty;
            AccessRole = string.Empty;
            AccessResource = string.Empty;
            SecurityRiskLevel = string.Empty;
            SecurityRequiresAudit = false;
            IntegrationEndpoint = string.Empty;
            IntegrationSystem = string.Empty;

            LoadPerformerOptions();

            if (currentUser is not null &&
                PerformerOptions.Any(p => string.Equals(p, currentUser.UserFullName, StringComparison.OrdinalIgnoreCase)))
            {
                SelectedPerformer = currentUser.UserFullName;
            }
            else
            {
                SelectedPerformer = PerformerOptions.FirstOrDefault() ?? "Не назначен";
            }

            SetSelectedServiceProfile(null);
        }

        private void FillFromTask(SDTask task)
        {
            LoadReferenceOptions();
            LoadServiceProfileOptions();
            SelectedTaskType = task.TaskTypeName;
            UserQueryId = task.UserQueryId;
            DateReg = task.DateReg;
            Priority = task.Priority;
            UserFio = task.UserFio;
            EnsureOption(DepartmentOptions, task.UserDepartName);
            UserDepartName = task.UserDepartName;
            UserQueryTag = task.UserQueryTag;
            EnsureOption(QueryTypeOptions, task.QueryTypeName);
            QueryTypeName = task.QueryTypeName;
            EnsureOption(ItProjectOptions, task.ItProjectName);
            ItProjectName = task.ItProjectName;
            ShortDescription = task.ShortDescription;
            StateName = task.StateName;
            DateNeedClose = task.DateNeedClose;
            PerformerName = task.PerformerName;
            PerformerDepartName = task.PerformerDepartName;
            PerformPercent = task.PerformPercent;
            DateClosed = task.DateClosed ?? DateTime.Now;

            ItSystemArea = string.Empty;
            ItRequiresDeployment = false;
            HardwareModel = string.Empty;
            HardwareAssetNumber = string.Empty;
            CommunicationChannel = string.Empty;
            CommunicationContact = string.Empty;
            AccessRole = string.Empty;
            AccessResource = string.Empty;
            SecurityRiskLevel = string.Empty;
            SecurityRequiresAudit = false;
            IntegrationEndpoint = string.Empty;
            IntegrationSystem = string.Empty;

            switch (task)
            {
                case ITTask itTask:
                    ItSystemArea = itTask.SystemArea;
                    ItRequiresDeployment = itTask.RequiresDeployment;
                    break;
                case HardwareTask hardwareTask:
                    HardwareModel = hardwareTask.EquipmentModel;
                    HardwareAssetNumber = hardwareTask.AssetNumber;
                    break;
                case CommunicationTask communicationTask:
                    CommunicationChannel = communicationTask.Channel;
                    CommunicationContact = communicationTask.ContactPoint;
                    break;
                case AccessTask accessTask:
                    AccessRole = accessTask.AccessRole;
                    AccessResource = accessTask.ResourceName;
                    break;
                case SecurityTask securityTask:
                    SecurityRiskLevel = securityTask.RiskLevel;
                    SecurityRequiresAudit = securityTask.RequiresAudit;
                    break;
                case IntegrationTask integrationTask:
                    IntegrationEndpoint = integrationTask.EndpointName;
                    IntegrationSystem = integrationTask.IntegrationSystem;
                    break;
            }

            LoadPerformerOptions();
            SelectedPerformer = PerformerOptions.FirstOrDefault(p =>
                                    string.Equals(p, task.PerformerName, StringComparison.OrdinalIgnoreCase))
                               ?? PerformerOptions.FirstOrDefault()
                               ?? task.PerformerName;
            SetSelectedServiceProfile(task.ServiceProfileId);
        }

        private void LoadReferenceOptions()
        {
            PopulateOptions(DepartmentOptions, _taskReferenceDataService.GetDepartments());
            PopulateOptions(QueryTypeOptions, _taskReferenceDataService.GetQueryTypes());
            PopulateOptions(ItProjectOptions, _taskReferenceDataService.GetItProjects());
        }

        private static void PopulateOptions(
            ObservableCollection<string> target,
            IReadOnlyList<string> source)
        {
            target.Clear();

            foreach (string value in source.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                target.Add(value);
            }
        }

        private static void EnsureOption(ObservableCollection<string> options, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!options.Any(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase)))
            {
                options.Add(value);
            }
        }

        private static string ChooseOption(
            IEnumerable<string> options,
            string? preferredValue)
        {
            List<string> list = options.ToList();

            if (!string.IsNullOrWhiteSpace(preferredValue))
            {
                string? preferredMatch = list.FirstOrDefault(v =>
                    string.Equals(v, preferredValue, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(preferredMatch))
                {
                    return preferredMatch;
                }
            }

            return list.FirstOrDefault() ?? string.Empty;
        }

        private void LoadPerformerOptions()
        {
            PerformerOptions.Clear();

            IReadOnlyList<UserInfo> options = _userDirectoryService.GetAssignableUsers(_currentUserContext.CurrentUser);
            foreach (UserInfo user in options.OrderBy(u => u.UserFullName))
            {
                PerformerOptions.Add(user.UserFullName);
            }

            if (PerformerOptions.Count == 0)
            {
                PerformerOptions.Add("Не назначен");
            }
        }

        private void LoadServiceProfileOptions()
        {
            int? previousProfileId = SelectedServiceProfile?.Id;

            ServiceProfileOptions.Clear();
            ServiceProfileOptions.Add(new ServiceProfileOption
            {
                Id = null,
                DisplayName = "Без профиля услуги"
            });

            foreach (IServiceProfileFlyweight profile in _serviceProfileFlyweightFactory.GetAll())
            {
                ServiceProfileOptions.Add(new ServiceProfileOption
                {
                    Id = profile.Id,
                    DisplayName = $"{profile.ServiceName} [{profile.ServiceCode}]"
                });
            }

            SetSelectedServiceProfile(previousProfileId);
        }

        private void SetSelectedServiceProfile(int? serviceProfileId, bool applyProfile = false)
        {
            _isSyncingServiceProfileSelection = true;
            SelectedServiceProfile = ServiceProfileOptions.FirstOrDefault(option => option.Id == serviceProfileId)
                                     ?? ServiceProfileOptions.FirstOrDefault(option => option.Id is null);
            _isSyncingServiceProfileSelection = false;

            if (applyProfile &&
                serviceProfileId.HasValue &&
                SelectedServiceProfile?.Id == serviceProfileId)
            {
                OnSelectedServiceProfileChanged(SelectedServiceProfile);
            }
        }

        private ServiceProfileTaskContext CaptureServiceProfileContext()
        {
            return new ServiceProfileTaskContext
            {
                SelectedTaskType = SelectedTaskType,
                Priority = Priority,
                QueryTypeName = QueryTypeName,
                ItProjectName = ItProjectName,
                UserQueryTag = UserQueryTag,
                PerformerDepartName = PerformerDepartName,
                ShortDescription = ShortDescription,
                DateReg = DateReg,
                DateNeedClose = DateNeedClose
            };
        }

        private void ApplyServiceProfileContext(ServiceProfileTaskContext context)
        {
            EnsureOption(QueryTypeOptions, context.QueryTypeName);
            EnsureOption(ItProjectOptions, context.ItProjectName);
            EnsureOption(DepartmentOptions, context.PerformerDepartName);

            if (!string.IsNullOrWhiteSpace(context.SelectedTaskType) &&
                TaskTypes.Contains(context.SelectedTaskType))
            {
                SelectedTaskType = context.SelectedTaskType;
            }

            if (!string.IsNullOrWhiteSpace(context.Priority) &&
                Priorities.Contains(context.Priority))
            {
                Priority = context.Priority;
            }

            QueryTypeName = context.QueryTypeName;
            ItProjectName = context.ItProjectName;
            UserQueryTag = context.UserQueryTag;
            PerformerDepartName = context.PerformerDepartName;
            ShortDescription = context.ShortDescription;
            DateNeedClose = context.DateNeedClose;
        }

        private static int? TryParseServiceProfileId(IDictionary<string, object> query)
        {
            return query.TryGetValue("serviceProfileId", out var value) &&
                   int.TryParse(value?.ToString(), out int parsedId) &&
                   parsedId > 0
                ? parsedId
                : null;
        }

        private bool CanAccessDepartment(string departmentName)
        {
            UserInfo? currentUser = _currentUserContext.CurrentUser;
            if (currentUser is null || IsAdministrator(currentUser))
            {
                return true;
            }

            return string.Equals(currentUser.UserDepartName, departmentName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAdministrator(UserInfo user)
        {
            return user.UserRoleId == AdministratorRoleId ||
                   string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyTypeSpecific(SDTask task)
        {
            switch (task)
            {
                case ITTask itTask:
                    itTask.SystemArea = ItSystemArea;
                    itTask.RequiresDeployment = ItRequiresDeployment;
                    break;
                case HardwareTask hardwareTask:
                    hardwareTask.EquipmentModel = HardwareModel;
                    hardwareTask.AssetNumber = HardwareAssetNumber;
                    break;
                case CommunicationTask communicationTask:
                    communicationTask.Channel = CommunicationChannel;
                    communicationTask.ContactPoint = CommunicationContact;
                    break;
                case AccessTask accessTask:
                    accessTask.AccessRole = AccessRole;
                    accessTask.ResourceName = AccessResource;
                    break;
                case SecurityTask securityTask:
                    securityTask.RiskLevel = SecurityRiskLevel;
                    securityTask.RequiresAudit = SecurityRequiresAudit;
                    break;
                case IntegrationTask integrationTask:
                    integrationTask.EndpointName = IntegrationEndpoint;
                    integrationTask.IntegrationSystem = IntegrationSystem;
                    break;
            }
        }

    }
}

