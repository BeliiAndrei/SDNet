using System.Data;
using Microsoft.Data.SqlClient;

namespace SDNet.Services.TaskStatusAudit
{
    public sealed class SqlTaskStatusChangeAuditComponent : TaskStatusChangeAuditComponent
    {
        public override void Save(TaskStatusChangeAuditRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);

            if (record.TaskId == Guid.Empty ||
                string.IsNullOrWhiteSpace(record.OldStateName) ||
                string.IsNullOrWhiteSpace(record.NewStateName) ||
                string.Equals(record.OldStateName, record.NewStateName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!SqlConnectionContext.Instance.IsInitialized)
            {
                throw new InvalidOperationException("SQL connection is not initialized.");
            }

            using var connection = new SqlConnection(SqlConnectionContext.Instance.ConnectionString);
            connection.Open();

            using var command = new SqlCommand("dbo.sp_SDTaskStatusChange_Add", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.Add(new SqlParameter("@TaskId", record.TaskId));
            command.Parameters.Add(new SqlParameter("@UserQueryId", record.UserQueryId > 0 ? record.UserQueryId : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@OldStateName", record.OldStateName.Trim()));
            command.Parameters.Add(new SqlParameter("@NewStateName", record.NewStateName.Trim()));
            command.Parameters.Add(new SqlParameter("@ChangedByLogin", DbValue(record.ChangedByLogin)));
            command.Parameters.Add(new SqlParameter("@ChangedByName", DbValue(record.ChangedByName)));
            command.Parameters.Add(new SqlParameter("@ChangedAt", record.ChangedAt == default ? DateTime.Now : record.ChangedAt));

            command.ExecuteNonQuery();
        }

        private static object DbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }
    }
}
