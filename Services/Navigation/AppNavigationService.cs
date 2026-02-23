namespace SDNet.Services.Navigation
{
    public sealed class AppNavigationService : IAppNavigationService
    {
        public event EventHandler? OpenShellRequested;

        public void RequestOpenShell()
        {
            OpenShellRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
