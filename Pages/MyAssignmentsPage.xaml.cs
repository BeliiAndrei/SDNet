namespace SDNet.Pages
{
    public partial class MyAssignmentsPage : ContentPage
    {
        public MyAssignmentsPage(MyAssignmentsPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
