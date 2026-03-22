namespace SDNet.Models.ServiceCatalog
{
    public sealed class ServiceCatalogCategory : ServiceCatalogComponent
    {
        private readonly List<ServiceCatalogComponent> _children = [];

        public ServiceCatalogCategory(
            int id,
            string name,
            string code,
            string description = "")
            : base(id, name, code, description)
        {
        }

        public override bool IsComposite => true;

        public override IReadOnlyList<ServiceCatalogComponent> Children => _children;

        public override void Add(ServiceCatalogComponent component)
        {
            ArgumentNullException.ThrowIfNull(component);

            if (ReferenceEquals(component, this))
            {
                throw new InvalidOperationException("A category cannot contain itself.");
            }

            if (component is ServiceCatalogCategory category && category.Contains(this))
            {
                throw new InvalidOperationException("Cannot create a cyclic composite graph.");
            }

            if (component.Parent is not null)
            {
                throw new InvalidOperationException("Catalog node already belongs to another category.");
            }

            component.AttachParent(this);
            _children.Add(component);
        }

        public override void Remove(int componentId)
        {
            ServiceCatalogComponent? component = _children.FirstOrDefault(child => child.Id == componentId);
            if (component is null)
            {
                return;
            }

            component.AttachParent(null);
            _children.Remove(component);
        }

        public int CountServices()
        {
            return Traverse().OfType<ServiceCatalogServiceItem>().Count();
        }

        private bool Contains(ServiceCatalogComponent candidate)
        {
            return Traverse().Any(node => ReferenceEquals(node, candidate));
        }
    }
}
