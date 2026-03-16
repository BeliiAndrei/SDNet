namespace SDNet.Pages
{
    public partial class TaskStatusHistoryPage : ContentPage
    {
        public TaskStatusHistoryPage(TaskStatusHistoryPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
