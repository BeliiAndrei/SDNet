using Microsoft.Data.SqlClient;
using SDNet.Models;
using SDNEt.BDParser;

namespace SDNet.Services.Auth
{
    internal static class UserInfoDbMapper
    {
        private static readonly UserInfoDirector Director = new();

        public static UserInfo Map(SqlDataReader reader)
        {
            var data = new UserInfoBuildData
            {
                UserId = reader.AsInt("UserId"),
                UserFullName = reader.AsString("UserFullName"),
                UserName = reader.AsString("UserName"),
                UserRoleId = reader.AsInt("UserRoleId"),
                UserRoleName = reader.AsString("UserRoleName"),
                UserDepartId = reader.AsInt("UserDepartId"),
                UserDepartName = reader.AsString("UserDepartName"),
                Email = reader.AsString("Email"),
                PhoneNumber = reader.AsString("PhoneNumber"),
                IsActive = reader.AsBool("IsActive", true),
                AuthorizedAt = reader.AsDateTime("AuthorizedAt", DateTime.Now),
                LastActivityAt = reader.AsNullableDateTime("LastActivityAt")
            };

            return Director.BuildFromDatabase(new DbUserInfoBuilder(), data);
        }
    }
}
