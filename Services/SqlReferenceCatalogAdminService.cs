using System.Data;
using Microsoft.Data.SqlClient;
using SDNet.Models;
using SDNEt.BDParser;

namespace SDNet.Services
{
    public sealed class SqlReferenceCatalogAdminService : IReferenceCatalogAdminService
    {
        public IReadOnlyList<ReferenceValue> GetDepartments()
        {
            return ExecuteList("dbo.sp_Department_ListActive");
        }

        public IReadOnlyList<ReferenceValue> GetQueryTypes()
        {
            return ExecuteList("dbo.sp_QueryType_ListActive");
        }

        public IReadOnlyList<ReferenceValue> GetItProjects()
        {
            return ExecuteList("dbo.sp_ItProject_ListActive");
        }

        public ReferenceValue AddDepartment(string name, string code)
        {
            return ExecuteSave("dbo.sp_Department_Add", name, code);
        }

        public ReferenceValue AddQueryType(string name, string code)
        {
            return ExecuteSave("dbo.sp_QueryType_Add", name, code);
        }

        public ReferenceValue AddItProject(string name, string code)
        {
            return ExecuteSave("dbo.sp_ItProject_Add", name, code);
        }

        public void DeleteDepartment(int id)
        {
            ExecuteDelete("dbo.sp_Department_Delete", id);
        }

        public void DeleteQueryType(int id)
        {
            ExecuteDelete("dbo.sp_QueryType_Delete", id);
        }

        public void DeleteItProject(int id)
        {
            ExecuteDelete("dbo.sp_ItProject_Delete", id);
        }

        private static IReadOnlyList<ReferenceValue> ExecuteList(string procedureName)
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            using SqlDataReader reader = command.ExecuteReader();
            var values = new List<ReferenceValue>();
            while (reader.Read())
            {
                values.Add(MapReferenceValue(reader));
            }

            return values;
        }

        private static ReferenceValue ExecuteSave(string procedureName, string name, string code)
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.Add(new SqlParameter("@Name", string.IsNullOrWhiteSpace(name) ? DBNull.Value : name.Trim()));
            command.Parameters.Add(new SqlParameter("@Code", string.IsNullOrWhiteSpace(code) ? DBNull.Value : code.Trim()));

            using SqlDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException("Справочник не был сохранен.");
            }

            return MapReferenceValue(reader);
        }

        private static void ExecuteDelete(string procedureName, int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            using var connection = CreateOpenConnection();
            using var command = new SqlCommand(procedureName, connection)
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

        private static ReferenceValue MapReferenceValue(SqlDataReader reader)
        {
            return new ReferenceValue
            {
                Id = reader.AsInt("Id"),
                Name = reader.AsString("Name"),
                Code = reader.AsString("Code")
            };
        }
    }
}
