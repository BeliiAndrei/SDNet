using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using SDNEt.BDParser;

namespace SDNet.Services
{
    public sealed class SqlTaskReferenceDataService : ITaskReferenceDataService
    {
        private readonly object _sync = new();
        private IReadOnlyList<string>? _departments;
        private IReadOnlyList<string>? _queryTypes;
        private IReadOnlyList<string>? _itProjects;
        private string _cachedConnectionString = string.Empty;

        public IReadOnlyList<string> GetDepartments()
        {
            return GetOrLoad(ref _departments, "dbo.sp_Department_ListActive");
        }

        public IReadOnlyList<string> GetQueryTypes()
        {
            return GetOrLoad(ref _queryTypes, "dbo.sp_QueryType_ListActive");
        }

        public IReadOnlyList<string> GetItProjects()
        {
            return GetOrLoad(ref _itProjects, "dbo.sp_ItProject_ListActive");
        }

        public void InvalidateCache()
        {
            lock (_sync)
            {
                _departments = null;
                _queryTypes = null;
                _itProjects = null;
                _cachedConnectionString = string.Empty;
            }
        }

        private IReadOnlyList<string> GetOrLoad(ref IReadOnlyList<string>? cache, string procedureName)
        {
            lock (_sync)
            {
                if (!SqlConnectionContext.Instance.IsInitialized)
                {
                    return [];
                }

                string currentConnectionString = SqlConnectionContext.Instance.ConnectionString;
                if (cache is not null &&
                    string.Equals(_cachedConnectionString, currentConnectionString, StringComparison.Ordinal))
                {
                    return cache;
                }

                cache = LoadListFromProcedure(procedureName);
                _cachedConnectionString = currentConnectionString;
                return cache;
            }
        }

        private static IReadOnlyList<string> LoadListFromProcedure(string procedureName)
        {
            if (!SqlConnectionContext.Instance.IsInitialized)
            {
                return [];
            }

            try
            {
                using var connection = new SqlConnection(SqlConnectionContext.Instance.ConnectionString);
                connection.Open();

                using var command = new SqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                using SqlDataReader reader = command.ExecuteReader();
                var items = new List<string>();
                while (reader.Read())
                {
                    string value = reader.AsString("Name").Trim();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    if (!items.Contains(value, StringComparer.OrdinalIgnoreCase))
                    {
                        items.Add(value);
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SqlTaskReferenceDataService] {procedureName} failed: {ex.Message}");
                return [];
            }
        }
    }
}

