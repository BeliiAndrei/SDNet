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
using SDNet.Services.TaskOperations;
using SDNet.Services.TaskPlanning;
using SDNet.Services.TaskWorkflow;

namespace SDNet.PageModels
{
    public partial class TaskEditorPageModel : ObservableObject, IQueryAttributable
    {
        private readonly ISDTaskStore _taskStore;
        private readonly CurrentUserContext _currentUserContext;
        private readonly IUserDirectoryService _userDirectoryService;
        private readonly ITaskReferenceDataService _taskReferenceDataService;
        private readonly ISDTaskFactoryMethodService _taskFactoryMethodService;
        private readonly IServiceProfileFlyweightFactory _serviceProfileFlyweightFactory;
        private readonly ITaskApplicationService _taskApplicationService;
        private readonly ITaskWorkflowService _taskWorkflowService;
        private readonly ITaskPlanningService _taskPlanningService;
        private readonly IUserSettingsService _userSettingsService;

        private Guid _taskId;
        private bool _isHydrating;
        private bool _isSyncingServiceProfileSelection;

        public IReadOnlyList<string> TaskTypes => _taskFactoryMethodService.SupportedTaskTypes;
        public IReadOnlyList<string> Priorities { get; } = ["Низкий", "Средний", "Высокий", "Критичный"];
        public ObservableCollection<string> PerformerOptions { get; } = [];
        public ObservableCollection<string> DepartmentOptions { get; } = [];
        public ObservableCollection<string> QueryTypeOptions { get; } = [];
        public ObservableCollection<string> ItProjectOptions { get; } = [];
        public ObservableCollection<ServiceProfileOption> ServiceProfileOptions { get; } = [];
        public ObservableCollection<TaskStateOption> StateOptions { get; } = [];

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
        [NotifyPropertyChangedFor(nameof(IsDateClosedVisible))]
        private TaskStateOption? _selectedStateOption;

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
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string _planningNote = string.Empty;

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
        public bool IsDateClosedVisible => SelectedStateOption?.Code == TaskStateCode.Closed;

        public TaskEditorPageModel(
            ISDTaskStore taskStore,
            CurrentUserContext currentUserContext,
            IUserDirectoryService userDirectoryService,
            ITaskReferenceDataService taskReferenceDataService,
            ISDTaskFactoryMethodService taskFactoryMethodService,
            IServiceProfileFlyweightFactory serviceProfileFlyweightFactory,
            ITaskApplicationService taskApplicationService,
            ITaskWorkflowService taskWorkflowService,
            ITaskPlanningService taskPlanningService,
            IUserSettingsService userSettingsService)
        {
            _taskStore = taskStore;
            _currentUserContext = currentUserContext;
            _userDirectoryService = userDirectoryService;
            _taskReferenceDataService = taskReferenceDataService;
            _taskFactoryMethodService = taskFactoryMethodService;
            _serviceProfileFlyweightFactory = serviceProfileFlyweightFactory;
            _taskApplicationService = taskApplicationService;
            _taskWorkflowService = taskWorkflowService;
            _taskPlanningService = taskPlanningService;
            _userSettingsService = userSettingsService;

            FillDefaults();
        }

        partial void OnSelectedTaskTypeChanged(string value)
        {
            OnPropertyChanged(nameof(IsITTask));
            OnPropertyChanged(nameof(IsHardwareTask));
            OnPropertyChanged(nameof(IsCommunicationTask));
            OnPropertyChanged(nameof(IsAccessTask));
            OnPropertyChanged(nameof(IsSecurityTask));
            OnPropertyChanged(nameof(IsIntegrationTask));

            if (_isHydrating)
            {
                return;
            }

            ApplyPlanningCore();
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
            if (_isHydrating || _isSyncingServiceProfileSelection || value?.Id is null)
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
            ApplyPlanningCore();
        }

        partial void OnSelectedStateOptionChanged(TaskStateOption? value)
        {
            OnPropertyChanged(nameof(IsDateClosedVisible));
            if (value?.Code == TaskStateCode.Closed)
            {
                DateClosed = DateTime.Now;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            bool isNew = query.TryGetValue("isNew", out var isNewValue) &&
                         bool.TryParse(isNewValue?.ToString(), out bool parsed) &&
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
                try
                {
                    SDTask? task = _taskStore.GetById(id);
                    if (task is null)
                    {
                        FillDefaults();
                        IsExistingTask = false;
                        return;
                    }

                    _taskId = task.Id;
                    IsExistingTask = true;
                    FillFromTask(task);
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    ShowMessage(ex.Message);
                    Shell.Current.GoToAsync("..").FireAndForgetSafeAsync();
                    FillDefaults();
                    IsExistingTask = false;
                    return;
                }
            }

            IsExistingTask = false;
            FillDefaults();
        }

        [RelayCommand]
        private async Task ApplyPlanning()
        {
            ApplyPlanningCore();
            await AppShell.DisplaySnackbarAsync(PlanningNote);
        }

        [RelayCommand]
        private async Task Save()
        {
            StatusMessage = string.Empty;

            UserInfo? currentUser = _currentUserContext.CurrentUser;
            if (currentUser is not null && !IsAdministrator(currentUser))
            {
                UserDepartName = currentUser.UserDepartName;
            }

            if (!IsExistingTask && !string.IsNullOrWhiteSpace(currentUser?.UserFullName))
            {
                UserFio = currentUser.UserFullName;
            }

            SDTask task = _taskFactoryMethodService.CreateTask(SelectedTaskType);
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
            task.StateId = (int)(SelectedStateOption?.Code ?? TaskStateCode.New);
            task.DateNeedClose = DateNeedClose;

            UserInfo? selectedPerformerUser = _userDirectoryService.GetByFullName(SelectedPerformer);
            task.PerformerName = selectedPerformerUser?.UserFullName ?? SelectedPerformer;
            task.PerformerDepartName = selectedPerformerUser?.UserDepartName ?? PerformerDepartName;
            task.PerformPercent = (int)Math.Round(PerformPercent);
            task.DateClosed = SelectedStateOption?.Code == TaskStateCode.Closed ? DateClosed : null;
            task.ServiceProfileId = SelectedServiceProfile?.Id;

            ApplyTypeSpecific(task);

            TaskOperationResult result = _taskApplicationService.Save(task, currentUser);
            if (!result.IsSuccessful)
            {
                await ShowMessageAsync(result.Message);
                return;
            }

            _taskId = task.Id;
            IsExistingTask = true;
            StatusMessage = "Задача сохранена.";
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

            UserSettings settings = await _userSettingsService.LoadAsync();
            if (settings.ConfirmBeforeDelete && Shell.Current is not null)
            {
                bool confirmed = await Shell.Current.DisplayAlert(
                    "Подтверждение удаления",
                    "Удалить текущую задачу?",
                    "Удалить",
                    "Отмена");

                if (!confirmed)
                {
                    return;
                }
            }

            TaskOperationResult result = _taskApplicationService.Delete(_taskId, _currentUserContext.CurrentUser);
            if (!result.IsSuccessful)
            {
                await ShowMessageAsync(result.Message);
                return;
            }

            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("..?refresh=true");
            }
        }

        [RelayCommand]
        private Task Cancel()
        {
            return Shell.Current is null
                ? Task.CompletedTask
                : Shell.Current.GoToAsync("..");
        }

        private void FillDefaults()
        {
            _isHydrating = true;
            try
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
                DateNeedClose = DateTime.Today.AddDays(2);
                PerformerName = string.Empty;
                PerformerDepartName = currentUser?.UserDepartName ?? "Service Desk";
                PerformPercent = 0;
                DateClosed = DateTime.Now;
                StatusMessage = string.Empty;
                PlanningNote = string.Empty;

                ResetTypeSpecificFields();
                LoadPerformerOptions();
                SelectedPerformer = PerformerOptions.FirstOrDefault(p =>
                    string.Equals(p, currentUser?.UserFullName, StringComparison.OrdinalIgnoreCase))
                    ?? PerformerOptions.FirstOrDefault()
                    ?? "Не назначен";

                SetSelectedServiceProfile(null);
                RefreshStateOptions(BuildDraftTask((int)TaskStateCode.New), (int)TaskStateCode.New);
            }
            finally
            {
                _isHydrating = false;
            }

            ApplyPlanningCore();
        }

        private void FillFromTask(SDTask task)
        {
            _isHydrating = true;
            try
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
                DateNeedClose = task.DateNeedClose;
                PerformerName = task.PerformerName;
                PerformerDepartName = task.PerformerDepartName;
                PerformPercent = task.PerformPercent;
                DateClosed = task.DateClosed ?? DateTime.Now;
                StatusMessage = string.Empty;

                ResetTypeSpecificFields();
                ApplyTypeSpecificFields(task);

                LoadPerformerOptions();
                SelectedPerformer = PerformerOptions.FirstOrDefault(p =>
                                        string.Equals(p, task.PerformerName, StringComparison.OrdinalIgnoreCase))
                                   ?? PerformerOptions.FirstOrDefault()
                                   ?? task.PerformerName;

                SetSelectedServiceProfile(task.ServiceProfileId);
                RefreshStateOptions(task, task.StateId);
            }
            finally
            {
                _isHydrating = false;
            }

            ApplyPlanningCore(updateFields: false);
        }

        private void LoadReferenceOptions()
        {
            PopulateOptions(DepartmentOptions, _taskReferenceDataService.GetDepartments());
            PopulateOptions(QueryTypeOptions, _taskReferenceDataService.GetQueryTypes());
            PopulateOptions(ItProjectOptions, _taskReferenceDataService.GetItProjects());
        }

        private void LoadPerformerOptions()
        {
            PerformerOptions.Clear();

            IReadOnlyList<UserInfo> options = _userDirectoryService.GetAssignableUsers(_currentUserContext.CurrentUser);
            foreach (UserInfo user in options.OrderBy(user => user.UserFullName))
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

        private void RefreshStateOptions(SDTask task, int selectedStateId)
        {
            StateOptions.Clear();

            IReadOnlyList<TaskStateOption> options = _taskWorkflowService.GetAvailableStates(task, _currentUserContext.CurrentUser);
            foreach (TaskStateOption option in options)
            {
                StateOptions.Add(option);
            }

            SelectedStateOption = StateOptions.FirstOrDefault(option => (int)option.Code == selectedStateId)
                                  ?? StateOptions.FirstOrDefault()
                                  ?? new TaskStateOption(TaskStateCode.New, TaskStateCatalog.GetName(TaskStateCode.New));
        }

        private void ApplyPlanningCore(bool updateFields = true)
        {
            TaskPlanningResult plan = _taskPlanningService.BuildPlan(new TaskPlanningRequest
            {
                TaskTypeName = SelectedTaskType,
                RegisteredAt = DateReg == default ? DateTime.Now : DateReg,
                CurrentPriority = Priority,
                CurrentPerformerDepartment = PerformerDepartName,
                CurrentUser = _currentUserContext.CurrentUser
            });

            PlanningNote = plan.Note;
            if (!updateFields)
            {
                return;
            }

            if (Priorities.Contains(plan.RecommendedPriority))
            {
                Priority = plan.RecommendedPriority;
            }

            EnsureOption(DepartmentOptions, plan.RecommendedPerformerDepartment);
            PerformerDepartName = plan.RecommendedPerformerDepartment;
            DateNeedClose = plan.RecommendedDueDate;
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

        private SDTask BuildDraftTask(int stateId)
        {
            SDTask task = _taskFactoryMethodService.CreateTask(SelectedTaskType);
            task.StateId = stateId;
            task.DateReg = DateReg;
            task.DateNeedClose = DateNeedClose;
            return task;
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

        private void ApplyTypeSpecificFields(SDTask task)
        {
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
        }

        private void ResetTypeSpecificFields()
        {
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
        }

        private static void PopulateOptions(ObservableCollection<string> target, IReadOnlyList<string> source)
        {
            target.Clear();
            foreach (string value in source.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
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

        private static string ChooseOption(IEnumerable<string> options, string? preferredValue)
        {
            List<string> list = options.ToList();
            if (!string.IsNullOrWhiteSpace(preferredValue))
            {
                string? preferredMatch = list.FirstOrDefault(item =>
                    string.Equals(item, preferredValue, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(preferredMatch))
                {
                    return preferredMatch;
                }
            }

            return list.FirstOrDefault() ?? string.Empty;
        }

        private static int? TryParseServiceProfileId(IDictionary<string, object> query)
        {
            return query.TryGetValue("serviceProfileId", out var value) &&
                   int.TryParse(value?.ToString(), out int parsedId) &&
                   parsedId > 0
                ? parsedId
                : null;
        }

        private static bool IsAdministrator(UserInfo user)
        {
            return user.UserRoleId == 1 ||
                   string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ShowMessageAsync(string message)
        {
            StatusMessage = message;
            await AppShell.DisplaySnackbarAsync(message);
        }

        private void ShowMessage(string message)
        {
            StatusMessage = message;
            AppShell.DisplaySnackbarAsync(message).FireAndForgetSafeAsync();
        }
    }
}
