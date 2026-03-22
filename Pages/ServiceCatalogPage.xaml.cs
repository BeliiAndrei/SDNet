namespace SDNet.Pages
{
    public partial class ServiceCatalogPage : ContentPage
    {
        public ServiceCatalogPage(ServiceCatalogPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
