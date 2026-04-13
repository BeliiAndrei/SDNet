using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SDNet.Services.Auth;
using SDNet.Services.Export;
using SDNet.Services.Navigation;
using SDNet.Services.Notifications;
using SDNet.Services.ServiceCatalog;
using SDNet.Services.ServiceProfiles;
using SDNet.Services.TaskCreation;
using SDNet.Services.TaskEvents;
using SDNet.Services.TaskMemento;
using SDNet.Services.TaskOperations;
using SDNet.Services.TaskPlanning;
using SDNet.Services.TaskStatusAudit;
using SDNet.Services.TaskWorkflow;
using SDNet.Services.Theming;
using Syncfusion.Maui.Toolkit.Hosting;

namespace SDNet
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if IOS || MACCATALYST
                    handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddSingleton<SDTaskCreator, ITTaskCreator>();
            builder.Services.AddSingleton<SDTaskCreator, HardwareTaskCreator>();
            builder.Services.AddSingleton<SDTaskCreator, CommunicationTaskCreator>();
            builder.Services.AddSingleton<SDTaskCreator, AccessTaskCreator>();
            builder.Services.AddSingleton<SDTaskCreator, SecurityTaskCreator>();
            builder.Services.AddSingleton<SDTaskCreator, IntegrationTaskCreator>();
            builder.Services.AddSingleton<ISDTaskFactoryMethodService, SDTaskFactoryMethodService>();

            builder.Services.AddSingleton<TaskState, NewTaskState>();
            builder.Services.AddSingleton<TaskState, InProgressTaskState>();
            builder.Services.AddSingleton<TaskState, ApprovalTaskState>();
            builder.Services.AddSingleton<TaskState, ClosedTaskState>();
            builder.Services.AddSingleton<ITaskStateFactory, TaskStateFactory>();
            builder.Services.AddSingleton<ITaskWorkflowService, TaskWorkflowService>();

            builder.Services.AddSingleton<ITaskPlanningStrategy, SecurityTaskPlanningStrategy>();
            builder.Services.AddSingleton<ITaskPlanningStrategy, IntegrationTaskPlanningStrategy>();
            builder.Services.AddSingleton<ITaskPlanningStrategy, HardwareTaskPlanningStrategy>();
            builder.Services.AddSingleton<ITaskPlanningStrategy, DefaultTaskPlanningStrategy>();
            builder.Services.AddSingleton<ITaskPlanningService, TaskPlanningService>();

            builder.Services.AddSingleton<SqlTaskStatusChangeAuditComponent>();
            builder.Services.AddSingleton<TaskStatusChangeAuditComponent>(sp =>
                new SafeTaskStatusChangeAuditDecorator(
                    new UserContextTaskStatusChangeAuditDecorator(
                        sp.GetRequiredService<SqlTaskStatusChangeAuditComponent>(),
                        sp.GetRequiredService<CurrentUserContext>())));
            builder.Services.AddSingleton<SqlTaskStatusChangeHistoryService>();
            builder.Services.AddSingleton<SqlSDTaskStore>();
            builder.Services.AddSingleton<ISDTaskStore, DepartmentScopedTaskStoreProxy>();

            builder.Services.AddSingleton<RequiredDescriptionTaskSaveHandler>();
            builder.Services.AddSingleton<DepartmentAccessTaskSaveHandler>();
            builder.Services.AddSingleton<PerformerAssignmentTaskSaveHandler>();
            builder.Services.AddSingleton<DueDateTaskSaveHandler>();
            builder.Services.AddSingleton<WorkflowTaskSaveHandler>();
            builder.Services.AddSingleton<CompletionTaskSaveHandler>();
            builder.Services.AddSingleton<ITaskSaveHandler>(sp =>
            {
                ITaskSaveHandler head = sp.GetRequiredService<RequiredDescriptionTaskSaveHandler>();
                head.SetNext(sp.GetRequiredService<DepartmentAccessTaskSaveHandler>())
                    .SetNext(sp.GetRequiredService<PerformerAssignmentTaskSaveHandler>())
                    .SetNext(sp.GetRequiredService<DueDateTaskSaveHandler>())
                    .SetNext(sp.GetRequiredService<WorkflowTaskSaveHandler>())
                    .SetNext(sp.GetRequiredService<CompletionTaskSaveHandler>());
                return head;
            });
            builder.Services.AddSingleton<ITaskValidationPipeline, TaskValidationPipeline>();
            builder.Services.AddSingleton<TaskStatusOriginator>();
            builder.Services.AddSingleton<ITaskStatusHistoryCaretaker, TaskStatusHistoryCaretaker>();

            builder.Services.AddSingleton<SqlNotificationHistoryService>();
            builder.Services.AddSingleton<INotificationGateway, MockEmailNotificationGateway>();

            builder.Services.AddSingleton<TaskBoardCacheObserver>();
            builder.Services.AddSingleton<ITaskBoardReadService>(sp => sp.GetRequiredService<TaskBoardCacheObserver>());
            builder.Services.AddSingleton<ITaskObserver>(sp => sp.GetRequiredService<TaskBoardCacheObserver>());

            builder.Services.AddSingleton<TaskStatusHistoryCacheObserver>();
            builder.Services.AddSingleton<ITaskStatusChangeHistoryService>(sp => sp.GetRequiredService<TaskStatusHistoryCacheObserver>());
            builder.Services.AddSingleton<ITaskObserver>(sp => sp.GetRequiredService<TaskStatusHistoryCacheObserver>());

            builder.Services.AddSingleton<TaskNotificationObserver>();
            builder.Services.AddSingleton<INotificationHistoryService>(sp => sp.GetRequiredService<TaskNotificationObserver>());
            builder.Services.AddSingleton<ITaskObserver>(sp => sp.GetRequiredService<TaskNotificationObserver>());

            builder.Services.AddSingleton<ITaskEventSubject, TaskEventSubject>();
            builder.Services.AddSingleton<ITaskApplicationService, TaskApplicationService>();

            builder.Services.AddSingleton<ITaskReferenceDataService, SqlTaskReferenceDataService>();
            builder.Services.AddSingleton<ITaskExportService, TaskExportBridgeService>();
            builder.Services.AddSingleton<IReferenceCatalogAdminService, SqlReferenceCatalogAdminService>();
            builder.Services.AddSingleton<IServiceCatalogDataService, SqlServiceCatalogDataService>();
            builder.Services.AddSingleton<IServiceCatalogAdminService, SqlServiceCatalogAdminService>();
            builder.Services.AddSingleton<IServiceProfileFlyweightFactory, ServiceProfileFlyweightFactory>();
            builder.Services.AddSingleton<IUserDirectoryService, SqlUserDirectoryService>();
            builder.Services.AddSingleton<IAuthorizationService, SqlAuthorizationService>();
            builder.Services.AddSingleton<IAppNavigationService, AppNavigationService>();
            builder.Services.AddSingleton(sp =>
            {
                CurrentUserContext.Initialize(sp.GetRequiredService<IAuthorizationService>());
                return CurrentUserContext.Instance;
            });
            builder.Services.AddSingleton<IUserSettingsService, UserSettingsService>();
            builder.Services.AddSingleton<IThemeService, ThemeService>();
            builder.Services.AddSingleton<ModalErrorHandler>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<LoginPageModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddSingleton<TaskListPageModel>();
            builder.Services.AddSingleton<OperationsDashboardPageModel>();
            builder.Services.AddSingleton<MyAssignmentsPageModel>();
            builder.Services.AddSingleton<SettingsPageModel>();
            builder.Services.AddSingleton<ManageUsersPageModel>();
            builder.Services.AddSingleton<ManageReferencesPageModel>();
            builder.Services.AddSingleton<ServiceCatalogPageModel>();
            builder.Services.AddSingleton<TaskStatusHistoryPageModel>();
            builder.Services.AddSingleton<NotificationHistoryPageModel>();
            builder.Services.AddSingleton<ManageReferencesPage>();
            builder.Services.AddSingleton<OperationsDashboardPage>();
            builder.Services.AddSingleton<MyAssignmentsPage>();
            builder.Services.AddSingleton<ServiceCatalogPage>();
            builder.Services.AddSingleton<TaskStatusHistoryPage>();
            builder.Services.AddSingleton<NotificationHistoryPage>();
            builder.Services.AddTransient<TaskEditorPageModel>();

            builder.Services.AddTransientWithShellRoute<TaskEditorPage, TaskEditorPageModel>("sdtask-edit");

            return builder.Build();
        }
    }
}
