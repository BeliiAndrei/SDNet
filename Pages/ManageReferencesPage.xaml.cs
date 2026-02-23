namespace SDNet.Pages
{
    public partial class ManageReferencesPage : ContentPage
    {
        public ManageReferencesPage(ManageReferencesPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
