namespace SDNet.Pages
{
    public partial class NotificationHistoryPage : ContentPage
    {
        public NotificationHistoryPage(NotificationHistoryPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
