namespace SDNet.Models
{
    public sealed class DictionaryUserRoleResolver : IUserRoleResolver
    {
        private readonly Dictionary<string, int> _roleIdByName;
        private readonly Dictionary<int, string> _roleNameById;

        public DictionaryUserRoleResolver(IEnumerable<KeyValuePair<int, string>> roles)
        {
            ArgumentNullException.ThrowIfNull(roles);

            _roleIdByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _roleNameById = new Dictionary<int, string>();

            foreach (KeyValuePair<int, string> role in roles)
            {
                string name = Normalize(role.Value);
                if (role.Key <= 0 || string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                _roleIdByName[name] = role.Key;
                _roleNameById[role.Key] = name;
            }
        }

        public bool TryResolveRoleId(string roleName, out int roleId)
        {
            return _roleIdByName.TryGetValue(Normalize(roleName), out roleId);
        }

        public bool TryResolveRoleName(int roleId, out string roleName)
        {
            return _roleNameById.TryGetValue(roleId, out roleName!);
        }

        private static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
