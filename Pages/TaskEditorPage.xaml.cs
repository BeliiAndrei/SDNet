namespace SDNet.Pages
{
    public partial class TaskEditorPage : ContentPage
    {
        public TaskEditorPage(TaskEditorPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
