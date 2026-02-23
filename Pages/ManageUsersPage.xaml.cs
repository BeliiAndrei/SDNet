namespace SDNet.Pages
{
    public partial class ManageUsersPage : ContentPage
    {
        public ManageUsersPage(ManageUsersPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
