using Microsoft.Data.SqlClient;
using SDNet.Models;
using SDNEt.BDParser;

namespace SDNet.Services.Auth
{
    internal static class UserInfoDbMapper
    {
        public static UserInfo Map(SqlDataReader reader)
        {
            return UserInfoBuilder.Create()
                .WithUserId(reader.AsInt("UserId"))
                .WithUserFullName(reader.AsString("UserFullName"))
                .WithUserName(reader.AsString("UserName"))
                .WithRole(reader.AsInt("UserRoleId"), reader.AsString("UserRoleName"))
                .WithDepartment(reader.AsInt("UserDepartId"), reader.AsString("UserDepartName"))
                .WithEmail(reader.AsString("Email"))
                .WithPhoneNumber(reader.AsString("PhoneNumber"))
                .WithIsActive(reader.AsBool("IsActive", true))
                .WithAuthorizedAt(reader.AsDateTime("AuthorizedAt", DateTime.Now))
                .WithLastActivityAt(reader.AsNullableDateTime("LastActivityAt"))
                .Build();
        }
    }
}
