using System.Data;
using Microsoft.Data.SqlClient;
using SDNet.Models;
using SDNet.Services;

namespace SDNet.Services.Auth
{
    public sealed class SqlAuthorizationService : IAuthorizationService
    {
        public async Task<UserInfo> AuthorizeAsync(string login, string password, CancellationToken cancellationToken = default)
        {
            string normalizedLogin = string.IsNullOrWhiteSpace(login)
                ? string.Empty
                : login.Trim();

            if (string.IsNullOrWhiteSpace(normalizedLogin))
            {
                throw new UnauthorizedAccessException("Введите логин.");
            }

            if (!SqlConnectionContext.Instance.IsInitialized)
            {
                throw new InvalidOperationException("Подключение к SQL Server не инициализировано.");
            }

            await using var connection = new SqlConnection(SqlConnectionContext.Instance.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var cmd = new SqlCommand("dbo.sp_UserInfo_GetByLogin", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };
            cmd.Parameters.Add(new SqlParameter("@Login", normalizedLogin));

            await using SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new UnauthorizedAccessException("Неверный логин или пароль.");
            }

            UserInfo user = UserInfoDbMapper.Map(reader);
            await reader.CloseAsync();

            await using var markCmd = new SqlCommand("dbo.sp_UserInfo_MarkAuthorized", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };
            markCmd.Parameters.Add(new SqlParameter("@Login", normalizedLogin));
            await markCmd.ExecuteNonQueryAsync(cancellationToken);

            user.LastActivityAt = DateTime.Now;
            return user;
        }
    }
}
