using System.Data;
using Microsoft.Data.SqlClient;
using SDNEt.BDParser;
using SDNet.Models;

namespace SDNet.Services.TaskStatusAudit
{
    public sealed class SqlTaskStatusChangeHistoryService : ITaskStatusChangeHistoryService
    {
        public IReadOnlyList<TaskStatusChangeHistoryItem> GetHistory(int? userQueryId = null)
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_SDTaskStatusChange_Select", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };
            command.Parameters.Add(new SqlParameter("@UserQueryId", userQueryId.HasValue ? userQueryId.Value : DBNull.Value));

            using SqlDataReader reader = command.ExecuteReader();
            var items = new List<TaskStatusChangeHistoryItem>();
            while (reader.Read())
            {
                items.Add(new TaskStatusChangeHistoryItem
                {
                    Id = reader.AsLong("Id"),
                    TaskId = reader.AsGuid("TaskId"),
                    UserQueryId = reader.AsInt("UserQueryId"),
                    OldStateName = reader.AsString("OldStateName"),
                    NewStateName = reader.AsString("NewStateName"),
                    ChangedByLogin = reader.AsString("ChangedByLogin"),
                    ChangedByName = reader.AsString("ChangedByName"),
                    ChangedAt = reader.AsDateTime("ChangedAt")
                });
            }

            return items;
        }

        private static SqlConnection CreateOpenConnection()
        {
            if (!SqlConnectionContext.Instance.IsInitialized)
            {
                throw new InvalidOperationException("SQL connection is not initialized.");
            }

            var connection = new SqlConnection(SqlConnectionContext.Instance.ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
