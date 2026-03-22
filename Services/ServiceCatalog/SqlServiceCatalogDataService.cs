using System.Data;
using Microsoft.Data.SqlClient;
using SDNet.Models.ServiceCatalog;
using SDNEt.BDParser;

namespace SDNet.Services.ServiceCatalog
{
    public sealed class SqlServiceCatalogDataService : IServiceCatalogDataService
    {
        private readonly object _sync = new();
        private ServiceCatalogCategory? _cachedCatalog;
        private string _cachedConnectionString = string.Empty;

        public ServiceCatalogCategory GetCatalog()
        {
            lock (_sync)
            {
                if (!SqlConnectionContext.Instance.IsInitialized)
                {
                    return CreateRoot();
                }

                string connectionString = SqlConnectionContext.Instance.ConnectionString;
                if (_cachedCatalog is not null &&
                    string.Equals(_cachedConnectionString, connectionString, StringComparison.Ordinal))
                {
                    return _cachedCatalog;
                }

                _cachedCatalog = LoadCatalog();
                _cachedConnectionString = connectionString;
                return _cachedCatalog;
            }
        }

        public void InvalidateCache()
        {
            lock (_sync)
            {
                _cachedCatalog = null;
                _cachedConnectionString = string.Empty;
            }
        }

        private static ServiceCatalogCategory LoadCatalog()
        {
            List<ServiceCatalogNodeRecord> nodes = LoadNodes();
            ServiceCatalogCategory root = CreateRoot();
            Dictionary<int, ServiceCatalogCategory> categories = [];

            foreach (ServiceCatalogNodeRecord node in nodes.Where(n => n.NodeType == ServiceCatalogNodeType.Category))
            {
                categories[node.Id] = new ServiceCatalogCategory(node.Id, node.Name, node.Code, node.Description);
            }

            foreach (ServiceCatalogNodeRecord node in nodes)
            {
                ServiceCatalogComponent component = node.NodeType switch
                {
                    ServiceCatalogNodeType.Category => categories[node.Id],
                    ServiceCatalogNodeType.Service => new ServiceCatalogServiceItem(
                        node.Id,
                        node.Name,
                        node.Code,
                        node.Description,
                        node.FulfillmentGroup,
                        node.RequestType,
                        node.EstimatedHours),
                    _ => throw new InvalidOperationException($"Unsupported catalog node type '{node.NodeType}'.")
                };

                if (node.ParentId.HasValue && categories.TryGetValue(node.ParentId.Value, out ServiceCatalogCategory? parent))
                {
                    parent.Add(component);
                }
                else
                {
                    root.Add(component);
                }
            }

            return root;
        }

        private static List<ServiceCatalogNodeRecord> LoadNodes()
        {
            if (!SqlConnectionContext.Instance.IsInitialized)
            {
                return [];
            }

            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_ServiceCatalogNode_ListActive", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            using SqlDataReader reader = command.ExecuteReader();
            List<ServiceCatalogNodeRecord> nodes = [];

            while (reader.Read())
            {
                int parentOrdinal = reader.GetOrdinal("ParentId");
                nodes.Add(new ServiceCatalogNodeRecord
                {
                    Id = reader.AsInt("Id"),
                    ParentId = reader.IsDBNull(parentOrdinal) ? null : reader.GetInt32(parentOrdinal),
                    NodeType = ParseNodeType(reader.AsString("NodeType")),
                    Name = reader.AsString("Name"),
                    Code = reader.AsString("Code"),
                    Description = reader.AsString("Description"),
                    FulfillmentGroup = reader.AsString("FulfillmentGroup"),
                    RequestType = reader.AsString("RequestType"),
                    EstimatedHours = reader.AsInt("EstimatedHours"),
                    DisplayOrder = reader.AsInt("DisplayOrder")
                });
            }

            return nodes
                .OrderBy(node => node.ParentId ?? 0)
                .ThenBy(node => node.DisplayOrder)
                .ThenBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static ServiceCatalogNodeType ParseNodeType(string value)
        {
            return string.Equals(value, "Service", StringComparison.OrdinalIgnoreCase)
                ? ServiceCatalogNodeType.Service
                : ServiceCatalogNodeType.Category;
        }

        private static ServiceCatalogCategory CreateRoot()
        {
            return new ServiceCatalogCategory(0, "Каталог услуг", "SERVICE_CATALOG_ROOT", "Корневой узел каталога услуг.");
        }

        private static SqlConnection CreateOpenConnection()
        {
            var connection = new SqlConnection(SqlConnectionContext.Instance.ConnectionString);
            connection.Open();
            return connection;
        }

        private enum ServiceCatalogNodeType
        {
            Category = 0,
            Service = 1
        }

        private sealed class ServiceCatalogNodeRecord
        {
            public int Id { get; init; }

            public int? ParentId { get; init; }

            public ServiceCatalogNodeType NodeType { get; init; }

            public string Name { get; init; } = string.Empty;

            public string Code { get; init; } = string.Empty;

            public string Description { get; init; } = string.Empty;

            public string FulfillmentGroup { get; init; } = string.Empty;

            public string RequestType { get; init; } = string.Empty;

            public int EstimatedHours { get; init; }

            public int DisplayOrder { get; init; }
        }
    }
}
