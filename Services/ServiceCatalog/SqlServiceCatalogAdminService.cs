using System.Data;
using Microsoft.Data.SqlClient;
using SDNet.Models.ServiceCatalog;
using SDNEt.BDParser;

namespace SDNet.Services.ServiceCatalog
{
    public sealed class SqlServiceCatalogAdminService : IServiceCatalogAdminService
    {
        public ServiceCatalogCategory AddCategory(
            string name,
            string code,
            string description,
            int? parentCategoryId)
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_ServiceCatalogCategory_Add", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.Add(new SqlParameter("@Name", string.IsNullOrWhiteSpace(name) ? DBNull.Value : name.Trim()));
            command.Parameters.Add(new SqlParameter("@Code", string.IsNullOrWhiteSpace(code) ? DBNull.Value : code.Trim()));
            command.Parameters.Add(new SqlParameter("@Description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : description.Trim()));
            command.Parameters.Add(new SqlParameter("@ParentId", parentCategoryId.HasValue ? parentCategoryId.Value : DBNull.Value));

            using SqlDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException("Категория каталога услуг не была сохранена.");
            }

            return new ServiceCatalogCategory(
                reader.AsInt("Id"),
                reader.AsString("Name"),
                reader.AsString("Code"),
                reader.AsString("Description"));
        }

        public void AddService(
            int parentCategoryId,
            string name,
            string code,
            string description,
            string fulfillmentGroup,
            string requestType,
            int estimatedHours,
            string defaultTaskTypeName,
            string defaultPriority,
            string defaultQueryTypeName,
            string defaultItProjectName,
            string defaultUserQueryTag,
            string defaultPerformerDepartName,
            string defaultShortDescription,
            int slaHours)
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_ServiceCatalogService_Add", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.Add(new SqlParameter("@ParentId", parentCategoryId));
            command.Parameters.Add(new SqlParameter("@Name", string.IsNullOrWhiteSpace(name) ? DBNull.Value : name.Trim()));
            command.Parameters.Add(new SqlParameter("@Code", string.IsNullOrWhiteSpace(code) ? DBNull.Value : code.Trim()));
            command.Parameters.Add(new SqlParameter("@Description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : description.Trim()));
            command.Parameters.Add(new SqlParameter("@FulfillmentGroup", string.IsNullOrWhiteSpace(fulfillmentGroup) ? DBNull.Value : fulfillmentGroup.Trim()));
            command.Parameters.Add(new SqlParameter("@RequestType", string.IsNullOrWhiteSpace(requestType) ? DBNull.Value : requestType.Trim()));
            command.Parameters.Add(new SqlParameter("@EstimatedHours", estimatedHours < 0 ? 0 : estimatedHours));
            command.Parameters.Add(new SqlParameter("@DefaultTaskTypeName", string.IsNullOrWhiteSpace(defaultTaskTypeName) ? DBNull.Value : defaultTaskTypeName.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultPriority", string.IsNullOrWhiteSpace(defaultPriority) ? DBNull.Value : defaultPriority.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultQueryTypeName", string.IsNullOrWhiteSpace(defaultQueryTypeName) ? DBNull.Value : defaultQueryTypeName.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultItProjectName", string.IsNullOrWhiteSpace(defaultItProjectName) ? DBNull.Value : defaultItProjectName.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultUserQueryTag", string.IsNullOrWhiteSpace(defaultUserQueryTag) ? DBNull.Value : defaultUserQueryTag.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultPerformerDepartName", string.IsNullOrWhiteSpace(defaultPerformerDepartName) ? DBNull.Value : defaultPerformerDepartName.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultShortDescription", string.IsNullOrWhiteSpace(defaultShortDescription) ? DBNull.Value : defaultShortDescription.Trim()));
            command.Parameters.Add(new SqlParameter("@SlaHours", slaHours < 0 ? 0 : slaHours));
            command.ExecuteNonQuery();
        }

        public void UpdateCategory(
            int id,
            string name,
            string code,
            string description)
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_ServiceCatalogCategory_Update", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.Add(new SqlParameter("@Id", id));
            command.Parameters.Add(new SqlParameter("@Name", string.IsNullOrWhiteSpace(name) ? DBNull.Value : name.Trim()));
            command.Parameters.Add(new SqlParameter("@Code", string.IsNullOrWhiteSpace(code) ? DBNull.Value : code.Trim()));
            command.Parameters.Add(new SqlParameter("@Description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : description.Trim()));
            command.ExecuteNonQuery();
        }

        public void UpdateService(
            int id,
            string name,
            string code,
            string description,
            string fulfillmentGroup,
            string requestType,
            int estimatedHours,
            string defaultTaskTypeName,
            string defaultPriority,
            string defaultQueryTypeName,
            string defaultItProjectName,
            string defaultUserQueryTag,
            string defaultPerformerDepartName,
            string defaultShortDescription,
            int slaHours)
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_ServiceCatalogService_Update", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.Add(new SqlParameter("@Id", id));
            command.Parameters.Add(new SqlParameter("@Name", string.IsNullOrWhiteSpace(name) ? DBNull.Value : name.Trim()));
            command.Parameters.Add(new SqlParameter("@Code", string.IsNullOrWhiteSpace(code) ? DBNull.Value : code.Trim()));
            command.Parameters.Add(new SqlParameter("@Description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : description.Trim()));
            command.Parameters.Add(new SqlParameter("@FulfillmentGroup", string.IsNullOrWhiteSpace(fulfillmentGroup) ? DBNull.Value : fulfillmentGroup.Trim()));
            command.Parameters.Add(new SqlParameter("@RequestType", string.IsNullOrWhiteSpace(requestType) ? DBNull.Value : requestType.Trim()));
            command.Parameters.Add(new SqlParameter("@EstimatedHours", estimatedHours < 0 ? 0 : estimatedHours));
            command.Parameters.Add(new SqlParameter("@DefaultTaskTypeName", string.IsNullOrWhiteSpace(defaultTaskTypeName) ? DBNull.Value : defaultTaskTypeName.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultPriority", string.IsNullOrWhiteSpace(defaultPriority) ? DBNull.Value : defaultPriority.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultQueryTypeName", string.IsNullOrWhiteSpace(defaultQueryTypeName) ? DBNull.Value : defaultQueryTypeName.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultItProjectName", string.IsNullOrWhiteSpace(defaultItProjectName) ? DBNull.Value : defaultItProjectName.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultUserQueryTag", string.IsNullOrWhiteSpace(defaultUserQueryTag) ? DBNull.Value : defaultUserQueryTag.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultPerformerDepartName", string.IsNullOrWhiteSpace(defaultPerformerDepartName) ? DBNull.Value : defaultPerformerDepartName.Trim()));
            command.Parameters.Add(new SqlParameter("@DefaultShortDescription", string.IsNullOrWhiteSpace(defaultShortDescription) ? DBNull.Value : defaultShortDescription.Trim()));
            command.Parameters.Add(new SqlParameter("@SlaHours", slaHours < 0 ? 0 : slaHours));
            command.ExecuteNonQuery();
        }

        public void DeleteNode(int id)
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_ServiceCatalogNode_Delete", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.Add(new SqlParameter("@Id", id));
            command.ExecuteNonQuery();
        }

        private static SqlConnection CreateOpenConnection()
        {
            if (!SqlConnectionContext.Instance.IsInitialized)
            {
                throw new InvalidOperationException("Подключение к базе данных не инициализировано.");
            }

            var connection = new SqlConnection(SqlConnectionContext.Instance.ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
