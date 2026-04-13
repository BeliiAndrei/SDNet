namespace SDNet.Pages
{
    public partial class OperationsDashboardPage : ContentPage
    {
        public OperationsDashboardPage(OperationsDashboardPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
