using System.Data;
using Microsoft.Data.SqlClient;
using SDNEt.BDParser;
using SDNet.Models;

namespace SDNet.Services.Notifications
{
    public interface INotificationHistoryService
    {
        IReadOnlyList<NotificationMessage> GetAll();
        void Save(NotificationMessage message);
    }

    public sealed class SqlNotificationHistoryService : INotificationHistoryService
    {
        public IReadOnlyList<NotificationMessage> GetAll()
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_NotificationMessage_List", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            using SqlDataReader reader = command.ExecuteReader();
            List<NotificationMessage> items = [];
            while (reader.Read())
            {
                items.Add(new NotificationMessage
                {
                    Id = reader.AsLong("Id"),
                    TaskId = reader.AsGuid("TaskId"),
                    UserQueryId = reader.AsNullableInt("UserQueryId"),
                    RecipientLogin = reader.AsString("RecipientLogin"),
                    RecipientName = reader.AsString("RecipientName"),
                    RecipientEmail = reader.AsString("RecipientEmail"),
                    Channel = reader.AsString("Channel"),
                    EventType = reader.AsString("EventType"),
                    Subject = reader.AsString("Subject"),
                    Body = reader.AsString("Body"),
                    Status = reader.AsString("Status"),
                    CreatedByLogin = reader.AsString("CreatedByLogin"),
                    CreatedByName = reader.AsString("CreatedByName"),
                    CreatedAt = reader.AsDateTime("CreatedAt"),
                    SentAt = reader.AsNullableDateTime("SentAt")
                });
            }

            return items;
        }

        public void Save(NotificationMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_NotificationMessage_Add", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.Add(new SqlParameter("@TaskId", message.TaskId));
            command.Parameters.Add(new SqlParameter("@UserQueryId", message.UserQueryId.HasValue ? message.UserQueryId.Value : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@RecipientLogin", DbValue(message.RecipientLogin)));
            command.Parameters.Add(new SqlParameter("@RecipientName", DbValue(message.RecipientName)));
            command.Parameters.Add(new SqlParameter("@RecipientEmail", DbValue(message.RecipientEmail)));
            command.Parameters.Add(new SqlParameter("@Channel", DbValue(message.Channel)));
            command.Parameters.Add(new SqlParameter("@EventType", DbValue(message.EventType)));
            command.Parameters.Add(new SqlParameter("@Subject", DbValue(message.Subject)));
            command.Parameters.Add(new SqlParameter("@Body", DbValue(message.Body)));
            command.Parameters.Add(new SqlParameter("@Status", DbValue(message.Status)));
            command.Parameters.Add(new SqlParameter("@CreatedByLogin", DbValue(message.CreatedByLogin)));
            command.Parameters.Add(new SqlParameter("@CreatedByName", DbValue(message.CreatedByName)));
            command.Parameters.Add(new SqlParameter("@CreatedAt", message.CreatedAt == default ? DateTime.Now : message.CreatedAt));
            command.Parameters.Add(new SqlParameter("@SentAt", message.SentAt.HasValue ? message.SentAt.Value : DBNull.Value));
            command.ExecuteNonQuery();
        }

        private static object DbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
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
