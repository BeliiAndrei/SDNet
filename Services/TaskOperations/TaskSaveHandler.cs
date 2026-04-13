using SDNet.Models;
using SDNet.Services.Auth;
using SDNet.Services.TaskWorkflow;

namespace SDNet.Services.TaskOperations
{
    public interface ITaskSaveHandler
    {
        ITaskSaveHandler SetNext(ITaskSaveHandler next);
        TaskOperationResult Handle(TaskSaveRequest request);
    }

    public abstract class TaskSaveHandler : ITaskSaveHandler
    {
        private ITaskSaveHandler? _next;

        public ITaskSaveHandler SetNext(ITaskSaveHandler next)
        {
            _next = next;
            return next;
        }

        public virtual TaskOperationResult Handle(TaskSaveRequest request)
        {
            return _next?.Handle(request) ?? TaskOperationResult.Success();
        }
    }

    public sealed class RequiredDescriptionTaskSaveHandler : TaskSaveHandler
    {
        public override TaskOperationResult Handle(TaskSaveRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Task.ShortDescription))
            {
                return TaskOperationResult.Fail("Заполните краткое описание задачи.");
            }

            return base.Handle(request);
        }
    }

    public sealed class DepartmentAccessTaskSaveHandler : TaskSaveHandler
    {
        private const int AdministratorRoleId = 1;

        public override TaskOperationResult Handle(TaskSaveRequest request)
        {
            if (request.CurrentUser is null)
            {
                return TaskOperationResult.Fail("Пользователь не авторизован.");
            }

            if (IsAdministrator(request.CurrentUser))
            {
                return base.Handle(request);
            }

            if (!string.Equals(
                    request.Task.UserDepartName,
                    request.CurrentUser.UserDepartName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return TaskOperationResult.Fail("Нельзя сохранять задачу другого подразделения.");
            }

            return base.Handle(request);
        }

        private static bool IsAdministrator(UserInfo user)
        {
            return user.UserRoleId == AdministratorRoleId ||
                   string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class PerformerAssignmentTaskSaveHandler : TaskSaveHandler
    {
        private const int AdministratorRoleId = 1;
        private readonly IUserDirectoryService _userDirectoryService;

        public PerformerAssignmentTaskSaveHandler(IUserDirectoryService userDirectoryService)
        {
            _userDirectoryService = userDirectoryService;
        }

        public override TaskOperationResult Handle(TaskSaveRequest request)
        {
            if (request.CurrentUser is null || IsAdministrator(request.CurrentUser))
            {
                return base.Handle(request);
            }

            UserInfo? selectedPerformer = _userDirectoryService.GetByFullName(request.Task.PerformerName);
            if (selectedPerformer is not null && IsAdministrator(selectedPerformer))
            {
                return TaskOperationResult.Fail("Пользователь с ролью User не может назначать задачу администратору.");
            }

            return base.Handle(request);
        }

        private static bool IsAdministrator(UserInfo user)
        {
            return user.UserRoleId == AdministratorRoleId ||
                   string.Equals(user.UserRoleName, "Administrator", StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class DueDateTaskSaveHandler : TaskSaveHandler
    {
        public override TaskOperationResult Handle(TaskSaveRequest request)
        {
            if (request.Task.DateNeedClose.Date < request.Task.DateReg.Date)
            {
                return TaskOperationResult.Fail("Срок закрытия не может быть раньше даты регистрации.");
            }

            return base.Handle(request);
        }
    }

    public sealed class WorkflowTaskSaveHandler : TaskSaveHandler
    {
        private readonly ITaskWorkflowService _taskWorkflowService;

        public WorkflowTaskSaveHandler(ITaskWorkflowService taskWorkflowService)
        {
            _taskWorkflowService = taskWorkflowService;
        }

        public override TaskOperationResult Handle(TaskSaveRequest request)
        {
            if (request.ExistingTask is null)
            {
                return base.Handle(request);
            }

            if (request.ExistingTask.StateId == request.Task.StateId)
            {
                return base.Handle(request);
            }

            try
            {
                if (request.IsUndoOperation)
                {
                    _taskWorkflowService.RestoreState(request.Task, request.CurrentUser, TaskStateCatalog.Normalize(request.Task.StateId));
                }
                else
                {
                    _taskWorkflowService.ApplyState(request.Task, request.CurrentUser, TaskStateCatalog.Normalize(request.Task.StateId));
                }
            }
            catch (Exception ex)
            {
                return TaskOperationResult.Fail(ex.Message);
            }

            return base.Handle(request);
        }
    }

    public sealed class CompletionTaskSaveHandler : TaskSaveHandler
    {
        public override TaskOperationResult Handle(TaskSaveRequest request)
        {
            TaskStateCode stateCode = TaskStateCatalog.Normalize(request.Task.StateId);
            if (stateCode == TaskStateCode.Closed && !request.Task.DateClosed.HasValue)
            {
                return TaskOperationResult.Fail("Для закрытой задачи должна быть указана дата закрытия.");
            }

            if (stateCode != TaskStateCode.Closed)
            {
                request.Task.DateClosed = null;
            }

            return base.Handle(request);
        }
    }
}
