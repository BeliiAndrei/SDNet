using System.Data;
using Microsoft.Data.SqlClient;
using SDNet.Models;
using SDNet.Services;

namespace SDNet.Services.Auth
{
    public sealed class SqlUserDirectoryService : IUserDirectoryService
    {
        private const int AdministratorRoleId = 1;

        public IReadOnlyList<UserInfo> GetAllUsers()
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_UserInfo_List", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };
            command.Parameters.Add(new SqlParameter("@OnlyActive", false));

            using SqlDataReader reader = command.ExecuteReader();
            return ReadUsers(reader);
        }

        public UserInfo? GetByLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return null;
            }

            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_UserInfo_GetByLoginAny", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };
            command.Parameters.Add(new SqlParameter("@Login", login.Trim()));

            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapUser(reader) : null;
        }

        public UserInfo? GetByFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_UserInfo_GetByFullName", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };
            command.Parameters.Add(new SqlParameter("@FullName", fullName.Trim()));
            command.Parameters.Add(new SqlParameter("@OnlyActive", true));

            using SqlDataReader reader = command.ExecuteReader();
            return reader.Read() ? MapUser(reader) : null;
        }

        public IReadOnlyList<UserInfo> GetAssignableUsers(UserInfo? currentUser)
        {
            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_UserInfo_List", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };
            command.Parameters.Add(new SqlParameter("@OnlyActive", true));

            using SqlDataReader reader = command.ExecuteReader();
            List<UserInfo> users = ReadUsers(reader).ToList();

            if (currentUser is null || IsAdministrator(currentUser))
            {
                return users;
            }

            return users
                .Where(u => !IsAdministrator(u) &&
                            string.Equals(u.UserDepartName, currentUser.UserDepartName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public UserInfo Save(UserInfo user)
        {
            ArgumentNullException.ThrowIfNull(user);

            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_UserInfo_Save", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            command.Parameters.Add(new SqlParameter("@UserId", user.UserId > 0 ? user.UserId : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Login", DbValue(user.UserName)));
            command.Parameters.Add(new SqlParameter("@FullName", DbValue(user.UserFullName)));
            command.Parameters.Add(new SqlParameter("@RoleId", user.UserRoleId > 0 ? user.UserRoleId : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@RoleName", DbValue(user.UserRoleName)));
            command.Parameters.Add(new SqlParameter("@DepartId", user.UserDepartId > 0 ? user.UserDepartId : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@DepartName", DbValue(user.UserDepartName)));
            command.Parameters.Add(new SqlParameter("@Email", DbValue(user.Email)));
            command.Parameters.Add(new SqlParameter("@PhoneNumber", DbValue(user.PhoneNumber)));
            command.Parameters.Add(new SqlParameter("@IsActive", user.IsActive));

            using SqlDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException("Не удалось сохранить пользователя.");
            }

            return MapUser(reader);
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

        private static IReadOnlyList<UserInfo> ReadUsers(SqlDataReader reader)
        {
            var users = new List<UserInfo>();
            while (reader.Read())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        private static UserInfo MapUser(SqlDataReader reader)
        {
            return UserInfoDbMapper.Map(reader);
        }

        private static bool IsAdministrator(UserInfo user)
        {
            return user.UserRoleId == AdministratorRoleId ||
                   string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase);
        }

    }
}
