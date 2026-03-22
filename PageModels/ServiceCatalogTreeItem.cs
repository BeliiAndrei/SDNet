using SDNet.Models.ServiceCatalog;

namespace SDNet.PageModels
{
    public sealed class ServiceCatalogTreeItem
    {
        public ServiceCatalogTreeItem(ServiceCatalogComponent component, int level, bool isExpanded)
        {
            Component = component;
            Id = component.Id;
            Name = component.Name;
            Code = component.Code;
            Description = component.Description;
            Level = level;
            IsCategory = component is ServiceCatalogCategory;
            IsExpanded = isExpanded;
            HasChildren = component.Children.Count > 0;
            Indent = new Thickness(level * 18, 0, 0, 0);
            IndentWidth = level * 22;
            Details = component switch
            {
                ServiceCatalogCategory category => $"Категория · {category.CountServices()} услуг",
                ServiceCatalogServiceItem service => $"{service.FulfillmentGroup} · {service.RequestType} · {service.EstimatedHours} ч",
                _ => string.Empty
            };
        }

        public ServiceCatalogComponent Component { get; }

        public int Id { get; }

        public string Name { get; }

        public string Code { get; }

        public string Description { get; }

        public int Level { get; }

        public bool IsCategory { get; }

        public bool IsExpanded { get; }

        public bool HasChildren { get; }

        public Thickness Indent { get; }

        public double IndentWidth { get; }

        public string Details { get; }

        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public string ExpandGlyph => !IsCategory ? string.Empty : IsExpanded ? "▾" : "▸";

        public string TypeLabel => IsCategory ? "Категория" : "Услуга";

        public string NodeGlyph => IsCategory ? "■" : "●";
    }
}
