using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SDNet.Services.Auth;
using SDNet.Services.Export;
using SDNet.Services.Navigation;
using SDNet.Services.TaskCreation;
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

            builder.Services.AddSingleton<ISDTaskStore, SqlSDTaskStore>();
            builder.Services.AddSingleton<ITaskReferenceDataService, SqlTaskReferenceDataService>();
            builder.Services.AddSingleton<ITaskExportService, TaskExportBridgeService>();
            builder.Services.AddSingleton<IReferenceCatalogAdminService, SqlReferenceCatalogAdminService>();
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
            builder.Services.AddSingleton<SettingsPageModel>();
            builder.Services.AddSingleton<ManageUsersPageModel>();
            builder.Services.AddSingleton<ManageReferencesPageModel>();
            builder.Services.AddSingleton<ManageReferencesPage>();
            builder.Services.AddTransient<TaskEditorPageModel>();

            builder.Services.AddTransientWithShellRoute<TaskEditorPage, TaskEditorPageModel>("sdtask-edit");

            return builder.Build();
        }
    }
}
