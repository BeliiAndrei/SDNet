namespace SDNet.Services.Navigation
{
    public interface IAppNavigationService
    {
        event EventHandler? OpenShellRequested;

        void RequestOpenShell();
    }
}
