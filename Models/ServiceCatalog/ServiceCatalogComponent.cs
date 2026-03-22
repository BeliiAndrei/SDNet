namespace SDNet.Models.ServiceCatalog
{
    public abstract class ServiceCatalogComponent
    {
        protected ServiceCatalogComponent(
            int id,
            string name,
            string code,
            string description = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Catalog node name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Catalog node code is required.", nameof(code));
            }

            Id = id;
            Name = name.Trim();
            Code = code.Trim();
            Description = description?.Trim() ?? string.Empty;
        }

        public int Id { get; }

        public string Name { get; }

        public string Code { get; }

        public string Description { get; }

        public ServiceCatalogCategory? Parent { get; private set; }

        public virtual bool IsComposite => false;

        public virtual IReadOnlyList<ServiceCatalogComponent> Children => [];

        public virtual void Add(ServiceCatalogComponent component)
        {
            throw new NotSupportedException("Leaf nodes cannot contain children.");
        }

        public virtual void Remove(int componentId)
        {
            throw new NotSupportedException("Leaf nodes cannot remove children.");
        }

        public IEnumerable<ServiceCatalogComponent> Traverse()
        {
            yield return this;

            foreach (ServiceCatalogComponent child in Children)
            {
                foreach (ServiceCatalogComponent nested in child.Traverse())
                {
                    yield return nested;
                }
            }
        }

        public string GetPath()
        {
            Stack<string> parts = [];
            ServiceCatalogComponent? current = this;

            while (current is not null)
            {
                parts.Push(current.Name);
                current = current.Parent;
            }

            return string.Join(" / ", parts);
        }

        internal void AttachParent(ServiceCatalogCategory? parent)
        {
            Parent = parent;
        }
    }
}
