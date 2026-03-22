using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SDNet.Models;
using SDNEt.BDParser;
using SDNet.Services;
using SDNet.Services.TaskCreation;
using SDNet.Services.TaskStatusAudit;

namespace SDNet.Data
{
    public sealed class SqlSDTaskStore : ISDTaskStore
    {
        private const int DefaultStartQueryId = 120001;
        private readonly ISDTaskFactoryMethodService _taskFactoryMethodService;
        private readonly TaskStatusChangeAuditComponent _taskStatusChangeAuditComponent;

        public SqlSDTaskStore(
            ISDTaskFactoryMethodService taskFactoryMethodService,
            TaskStatusChangeAuditComponent taskStatusChangeAuditComponent)
        {
            _taskFactoryMethodService = taskFactoryMethodService;
            _taskStatusChangeAuditComponent = taskStatusChangeAuditComponent;
        }

        public IReadOnlyList<SDTask> GetAll()
        {
            return ExecuteSafe(() =>
            {
                using var connection = CreateOpenConnection();
                using var command = new SqlCommand("dbo.sp_SDTask_List", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60
                };

                using SqlDataReader reader = command.ExecuteReader();
                var tasks = new List<SDTask>();
                while (reader.Read())
                {
                    tasks.Add(MapTask(reader));
                }

                return tasks;
            }, []);
        }

        public Task<IReadOnlyList<SDTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetAll());
        }

        public SDTask CreateNew(string taskTypeName)
        {
            var task = _taskFactoryMethodService.CreateTask(taskTypeName);
            FillDefaults(task);
            Save(task);
            return task;
        }

        public Task<SDTask> CreateNewAsync(string taskTypeName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CreateNew(taskTypeName));
        }

        public SDTask Clone(Guid id)
        {
            SDTask original = GetById(id) ?? throw new InvalidOperationException("Task not found.");
            SDTask clone = (SDTask)original.Clone();

            clone.Id = Guid.Empty;
            clone.UserQueryId = 0;
            clone.DateReg = DateTime.Now;
            clone.StateName = "Новая";
            clone.DateClosed = null;
            clone.PerformPercent = 0;
            clone.ShortDescription = $"{original.ShortDescription} (Копия)";

            Save(clone);
            return clone;
        }

        public Task<SDTask> CloneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Clone(id));
        }

        public SDTask? GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return null;
            }

            return ExecuteSafe(() =>
            {
                using var connection = CreateOpenConnection();
                using var command = new SqlCommand("dbo.sp_SDTask_GetById", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 60
                };
                command.Parameters.Add(new SqlParameter("@Id", id));

                using SqlDataReader reader = command.ExecuteReader();
                return reader.Read() ? MapTask(reader) : null;
            }, null);
        }

        public Task<SDTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetById(id));
        }

        public int PeekNextUserQueryId()
        {
            return ExecuteSafe(() =>
            {
                using var connection = CreateOpenConnection();
                using var command = new SqlCommand(
                    "SELECT ISNULL(MAX(UserQueryId), 120000) + 1 FROM dbo.SDTasks;",
                    connection);
                object? result = command.ExecuteScalar();
                return result is null || result is DBNull ? DefaultStartQueryId : Convert.ToInt32(result);
            }, DefaultStartQueryId);
        }

        public Task<int> PeekNextUserQueryIdAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(PeekNextUserQueryId());
        }

        public void Save(SDTask task)
        {
            ArgumentNullException.ThrowIfNull(task);
            SDTask? existingTask = task.Id == Guid.Empty ? null : GetById(task.Id);

            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_SDTask_Upsert", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };

            command.Parameters.Add(new SqlParameter("@Id", task.Id == Guid.Empty ? DBNull.Value : task.Id));
            command.Parameters.Add(new SqlParameter("@UserQueryId", task.UserQueryId <= 0 ? DBNull.Value : task.UserQueryId));
            command.Parameters.Add(new SqlParameter("@TaskTypeName", DbValue(task.TaskTypeName)));
            command.Parameters.Add(new SqlParameter("@DateReg", task.DateReg == default ? DBNull.Value : task.DateReg));
            command.Parameters.Add(new SqlParameter("@Priority", DbValue(task.Priority)));
            command.Parameters.Add(new SqlParameter("@UserFio", DbValue(task.UserFio)));
            command.Parameters.Add(new SqlParameter("@UserDepartName", DbValue(task.UserDepartName)));
            command.Parameters.Add(new SqlParameter("@UserQueryTag", DbValue(task.UserQueryTag)));
            command.Parameters.Add(new SqlParameter("@QueryTypeName", DbValue(task.QueryTypeName)));
            command.Parameters.Add(new SqlParameter("@ItProjectName", DbValue(task.ItProjectName)));
            command.Parameters.Add(new SqlParameter("@ShortDescription", DbValue(task.ShortDescription)));
            command.Parameters.Add(new SqlParameter("@StateName", DbValue(task.StateName)));
            command.Parameters.Add(new SqlParameter("@DateNeedClose", task.DateNeedClose == default ? DBNull.Value : task.DateNeedClose));
            command.Parameters.Add(new SqlParameter("@PerformerName", DbValue(task.PerformerName)));
            command.Parameters.Add(new SqlParameter("@PerformerDepartName", DbValue(task.PerformerDepartName)));
            command.Parameters.Add(new SqlParameter("@PerformPercent", Math.Clamp(task.PerformPercent, 0, 100)));
            command.Parameters.Add(new SqlParameter("@DateClosed", task.DateClosed.HasValue ? task.DateClosed.Value : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@ServiceProfileId", task.ServiceProfileId.HasValue ? task.ServiceProfileId.Value : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@NotesJson", task.Notes.Count > 0 ? JsonSerializer.Serialize(task.Notes) : DBNull.Value));

            command.Parameters.Add(new SqlParameter("@IT_SystemArea", task is ITTask itTask ? DbValue(itTask.SystemArea) : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@IT_RequiresDeployment", task is ITTask itTask2 ? itTask2.RequiresDeployment : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@HW_EquipmentModel", task is HardwareTask hardwareTask ? DbValue(hardwareTask.EquipmentModel) : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@HW_AssetNumber", task is HardwareTask hardwareTask2 ? DbValue(hardwareTask2.AssetNumber) : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@COM_Channel", task is CommunicationTask communicationTask ? DbValue(communicationTask.Channel) : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@COM_ContactPoint", task is CommunicationTask communicationTask2 ? DbValue(communicationTask2.ContactPoint) : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@ACC_AccessRole", task is AccessTask accessTask ? DbValue(accessTask.AccessRole) : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@ACC_ResourceName", task is AccessTask accessTask2 ? DbValue(accessTask2.ResourceName) : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@SEC_RiskLevel", task is SecurityTask securityTask ? DbValue(securityTask.RiskLevel) : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@SEC_RequiresAudit", task is SecurityTask securityTask2 ? securityTask2.RequiresAudit : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@INT_EndpointName", task is IntegrationTask integrationTask ? DbValue(integrationTask.EndpointName) : DBNull.Value));
            command.Parameters.Add(new SqlParameter("@INT_IntegrationSystem", task is IntegrationTask integrationTask2 ? DbValue(integrationTask2.IntegrationSystem) : DBNull.Value));

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                task.Id = reader.AsGuid("Id");
                task.UserQueryId = reader.AsInt("UserQueryId");
            }
            else
            {
                task.UserQueryId = task.UserQueryId <= 0 ? PeekNextUserQueryId() : task.UserQueryId;
            }

            if (existingTask is not null &&
                !string.Equals(existingTask.StateName, task.StateName, StringComparison.OrdinalIgnoreCase))
            {
                _taskStatusChangeAuditComponent.Save(new TaskStatusChangeAuditRecord
                {
                    TaskId = task.Id,
                    UserQueryId = task.UserQueryId,
                    OldStateName = existingTask.StateName,
                    NewStateName = task.StateName
                });
            }
        }

        public Task SaveAsync(SDTask task, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Save(task);
            return Task.CompletedTask;
        }

        public void Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return;
            }

            using var connection = CreateOpenConnection();
            using var command = new SqlCommand("dbo.sp_SDTask_Delete", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            command.Parameters.Add(new SqlParameter("@Id", id));
            command.ExecuteNonQuery();
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Delete(id);
            return Task.CompletedTask;
        }

        private static object DbValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
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

        private static T ExecuteSafe<T>(Func<T> action, T fallback)
        {
            if (!SqlConnectionContext.Instance.IsInitialized)
            {
                return fallback;
            }

            return action();
        }

        private SDTask MapTask(SqlDataReader reader)
        {
            string taskTypeName = reader.AsString("TaskTypeName");
            SDTask task = _taskFactoryMethodService.CreateTask(taskTypeName);

            task.Id = reader.AsGuid("Id");
            task.UserQueryId = reader.AsInt("UserQueryId");
            task.DateReg = reader.AsDateTime("DateReg", DateTime.MinValue);
            task.Priority = reader.AsString("Priority");
            task.UserFio = reader.AsString("UserFio");
            task.UserDepartName = reader.AsString("UserDepartName");
            task.UserQueryTag = reader.AsString("UserQueryTag");
            task.QueryTypeName = reader.AsString("QueryTypeName");
            task.ItProjectName = reader.AsString("ItProjectName");
            task.ShortDescription = reader.AsString("ShortDescription");
            task.StateName = reader.AsString("StateName");
            task.DateNeedClose = reader.AsDateTime("DateNeedClose", DateTime.MinValue);
            task.PerformerName = reader.AsString("PerformerName");
            task.PerformerDepartName = reader.AsString("PerformerDepartName");
            task.PerformPercent = reader.AsInt("PerformPercent");
            task.DateClosed = reader.AsNullableDateTime("DateClosed");
            task.ServiceProfileId = reader.AsNullableInt("ServiceProfileId");
            task.Notes = ParseNotes(reader.AsNullableString("NotesJson"));

            switch (task)
            {
                case ITTask itTask:
                    itTask.SystemArea = reader.AsString("IT_SystemArea");
                    itTask.RequiresDeployment = reader.AsBool("IT_RequiresDeployment");
                    break;
                case HardwareTask hardwareTask:
                    hardwareTask.EquipmentModel = reader.AsString("HW_EquipmentModel");
                    hardwareTask.AssetNumber = reader.AsString("HW_AssetNumber");
                    break;
                case CommunicationTask communicationTask:
                    communicationTask.Channel = reader.AsString("COM_Channel");
                    communicationTask.ContactPoint = reader.AsString("COM_ContactPoint");
                    break;
                case AccessTask accessTask:
                    accessTask.AccessRole = reader.AsString("ACC_AccessRole");
                    accessTask.ResourceName = reader.AsString("ACC_ResourceName");
                    break;
                case SecurityTask securityTask:
                    securityTask.RiskLevel = reader.AsString("SEC_RiskLevel");
                    securityTask.RequiresAudit = reader.AsBool("SEC_RequiresAudit");
                    break;
                case IntegrationTask integrationTask:
                    integrationTask.EndpointName = reader.AsString("INT_EndpointName");
                    integrationTask.IntegrationSystem = reader.AsString("INT_IntegrationSystem");
                    break;
            }

            return task;
        }

        private static List<string> ParseNotes(string? notesJson)
        {
            if (string.IsNullOrWhiteSpace(notesJson))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(notesJson) ?? [];
            }
            catch
            {
                return [];
            }
        }


        private static void FillDefaults(SDTask task)
        {
            DateTime now = DateTime.Now;
            task.Id = Guid.Empty;
            task.UserQueryId = 0;
            task.DateReg = now;
            task.Priority = "РЎСЂРµРґРЅРёР№";
            task.UserFio = "РќРѕРІС‹Р№ РїРѕР»СЊР·РѕРІР°С‚РµР»СЊ";
            task.UserDepartName = "Service Desk";
            task.UserQueryTag = "NEW";
            task.QueryTypeName = "Р—Р°РїСЂРѕСЃ РЅР° РѕР±СЃР»СѓР¶РёРІР°РЅРёРµ";
            task.ItProjectName = "SDNet";
            task.ShortDescription = "РќРѕРІР°СЏ Р·Р°РґР°С‡Р°";
            task.StateName = "РќРѕРІР°СЏ";
            task.DateNeedClose = now.AddDays(2);
            task.PerformerName = "РќРµ РЅР°Р·РЅР°С‡РµРЅ";
            task.PerformerDepartName = "Service Desk";
            task.PerformPercent = 0;
            task.DateClosed = null;
            task.ServiceProfileId = null;
        }

    }
}
