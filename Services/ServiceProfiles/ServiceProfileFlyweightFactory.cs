using System.Data;
using Microsoft.Data.SqlClient;
using SDNEt.BDParser;
using SDNet.Models.ServiceProfiles;
using SDNet.Services;

namespace SDNet.Services.ServiceProfiles
{
    public sealed class ServiceProfileFlyweightFactory : IServiceProfileFlyweightFactory
    {
        private Dictionary<int, IServiceProfileFlyweight>? _flyweights;

        public IReadOnlyList<IServiceProfileFlyweight> GetAll()
        {
            return EnsureFlyweights()
                .Values
                .OrderBy(profile => profile.ServiceName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IServiceProfileFlyweight? GetById(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return null;
            }

            return EnsureFlyweights().GetValueOrDefault(id.Value);
        }

        public IServiceProfileFlyweight? GetByServiceCatalogNodeId(int serviceCatalogNodeId)
        {
            if (serviceCatalogNodeId <= 0)
            {
                return null;
            }

            return EnsureFlyweights()
                .Values
                .FirstOrDefault(profile => profile.ServiceCatalogNodeId == serviceCatalogNodeId);
        }

        public void InvalidateCache()
        {
            _flyweights = null;
        }

        private Dictionary<int, IServiceProfileFlyweight> EnsureFlyweights()
        {
            if (_flyweights is not null)
            {
                return _flyweights;
            }

            _flyweights = LoadFlyweights();
            return _flyweights;
        }

        private static Dictionary<int, IServiceProfileFlyweight> LoadFlyweights()
        {
            Dictionary<int, IServiceProfileFlyweight> flyweights = [];
            if (!SqlConnectionContext.Instance.IsInitialized)
            {
                return flyweights;
            }

            using var connection = new SqlConnection(SqlConnectionContext.Instance.ConnectionString);
            connection.Open();

            using var command = new SqlCommand("dbo.sp_ServiceProfile_ListActive", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                IServiceProfileFlyweight flyweight = new ServiceProfileFlyweight(
                    reader.AsInt("Id"),
                    reader.AsInt("ServiceCatalogNodeId"),
                    reader.AsString("ServiceCode"),
                    reader.AsString("ServiceName"),
                    reader.AsString("ServiceDescription"),
                    reader.AsString("FulfillmentGroup"),
                    reader.AsString("RequestType"),
                    reader.AsInt("EstimatedHours"),
                    reader.AsString("DefaultTaskTypeName"),
                    reader.AsString("DefaultPriority"),
                    reader.AsString("DefaultQueryTypeName"),
                    reader.AsString("DefaultItProjectName"),
                    reader.AsString("DefaultUserQueryTag"),
                    reader.AsString("DefaultPerformerDepartName"),
                    reader.AsString("DefaultShortDescription"),
                    reader.AsInt("SlaHours"));

                flyweights[flyweight.Id] = flyweight;
            }

            return flyweights;
        }
    }
}
