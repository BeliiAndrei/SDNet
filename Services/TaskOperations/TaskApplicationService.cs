using SDNet.Data;
using SDNet.Models;
using SDNet.Services.TaskEvents;
using SDNet.Services.TaskMemento;
using SDNet.Services.TaskWorkflow;

namespace SDNet.Services.TaskOperations
{
    public interface ITaskApplicationService
    {
        TaskOperationResult Save(SDTask task, UserInfo? currentUser);
        TaskOperationResult Delete(Guid taskId, UserInfo? currentUser);
        TaskOperationResult Clone(Guid taskId, UserInfo? currentUser, out SDTask? clone);
        TaskOperationResult UndoLastStatusChange(Guid taskId, UserInfo? currentUser);
    }

    public sealed class TaskApplicationService : ITaskApplicationService
    {
        private readonly ISDTaskStore _taskStore;
        private readonly ITaskValidationPipeline _taskValidationPipeline;
        private readonly ITaskWorkflowService _taskWorkflowService;
        private readonly TaskStatusOriginator _taskStatusOriginator;
        private readonly ITaskStatusHistoryCaretaker _taskStatusHistoryCaretaker;
        private readonly ITaskEventSubject _taskEventSubject;

        public TaskApplicationService(
            ISDTaskStore taskStore,
            ITaskValidationPipeline taskValidationPipeline,
            ITaskWorkflowService taskWorkflowService,
            TaskStatusOriginator taskStatusOriginator,
            ITaskStatusHistoryCaretaker taskStatusHistoryCaretaker,
            ITaskEventSubject taskEventSubject)
        {
            _taskStore = taskStore;
            _taskValidationPipeline = taskValidationPipeline;
            _taskWorkflowService = taskWorkflowService;
            _taskStatusOriginator = taskStatusOriginator;
            _taskStatusHistoryCaretaker = taskStatusHistoryCaretaker;
            _taskEventSubject = taskEventSubject;
        }

        public TaskOperationResult Save(SDTask task, UserInfo? currentUser)
        {
            ArgumentNullException.ThrowIfNull(task);

            SDTask? existingTask = task.Id == Guid.Empty ? null : _taskStore.GetById(task.Id);
            if (existingTask is null)
            {
                _taskWorkflowService.RestoreState(task, currentUser, TaskStateCatalog.Normalize(task.StateId));
            }

            var request = new TaskSaveRequest
            {
                Task = task,
                ExistingTask = existingTask is null ? null : (SDTask)existingTask.Clone(),
                CurrentUser = currentUser
            };

            TaskOperationResult validationResult = _taskValidationPipeline.Validate(request);
            if (!validationResult.IsSuccessful)
            {
                return validationResult;
            }

            bool assignmentChanged = existingTask is null ||
                                     !string.Equals(existingTask.PerformerName, task.PerformerName, StringComparison.OrdinalIgnoreCase);
            bool stateChanged = existingTask is not null && existingTask.StateId != task.StateId;
            TaskStatusMemento? memento = null;

            if (stateChanged && existingTask is not null)
            {
                _taskStatusOriginator.SetTask(existingTask);
                memento = _taskStatusOriginator.CreateMemento();
            }

            try
            {
                _taskStore.Save(task);
                if (memento is not null)
                {
                    _taskStatusHistoryCaretaker.Push(memento);
                }

                PublishSaveEvents(task, existingTask, currentUser, assignmentChanged, stateChanged);
                return TaskOperationResult.Success();
            }
            catch (Exception ex)
            {
                return TaskOperationResult.Fail(ex.Message);
            }
        }

        public TaskOperationResult Delete(Guid taskId, UserInfo? currentUser)
        {
            if (taskId == Guid.Empty)
            {
                return TaskOperationResult.Success();
            }

            SDTask? existingTask = _taskStore.GetById(taskId);
            if (existingTask is null)
            {
                return TaskOperationResult.Success();
            }

            try
            {
                _taskStore.Delete(taskId);
                _taskEventSubject.Notify(new TaskDeletedDomainEvent((SDTask)existingTask.Clone(), currentUser));
                return TaskOperationResult.Success();
            }
            catch (Exception ex)
            {
                return TaskOperationResult.Fail(ex.Message);
            }
        }

        public TaskOperationResult Clone(Guid taskId, UserInfo? currentUser, out SDTask? clone)
        {
            clone = null;

            try
            {
                clone = _taskStore.Clone(taskId);
                PublishSaveEvents(clone, null, currentUser, assignmentChanged: true, stateChanged: false);
                return TaskOperationResult.Success();
            }
            catch (Exception ex)
            {
                return TaskOperationResult.Fail(ex.Message);
            }
        }

        public TaskOperationResult UndoLastStatusChange(Guid taskId, UserInfo? currentUser)
        {
            if (taskId == Guid.Empty)
            {
                return TaskOperationResult.Fail("Сначала выберите задачу.");
            }

            SDTask? task = _taskStore.GetById(taskId);
            if (task is null)
            {
                return TaskOperationResult.Fail("Задача не найдена.");
            }

            TaskStatusMemento? memento = _taskStatusHistoryCaretaker.PopLast(taskId);
            if (memento is null)
            {
                return TaskOperationResult.Fail("Для выбранной задачи нет сохраненного предыдущего состояния.");
            }

            SDTask previousSnapshot = (SDTask)task.Clone();
            _taskStatusOriginator.Restore(task, memento);

            var request = new TaskSaveRequest
            {
                Task = task,
                ExistingTask = previousSnapshot,
                CurrentUser = currentUser,
                IsUndoOperation = true
            };

            TaskOperationResult validationResult = _taskValidationPipeline.Validate(request);
            if (!validationResult.IsSuccessful)
            {
                _taskStatusHistoryCaretaker.Push(memento);
                return validationResult;
            }

            try
            {
                _taskStore.Save(task);
                PublishSaveEvents(task, previousSnapshot, currentUser, assignmentChanged: false, stateChanged: previousSnapshot.StateId != task.StateId);
                return TaskOperationResult.Success();
            }
            catch (Exception ex)
            {
                _taskStatusHistoryCaretaker.Push(memento);
                return TaskOperationResult.Fail(ex.Message);
            }
        }

        private void PublishSaveEvents(
            SDTask task,
            SDTask? previousTask,
            UserInfo? currentUser,
            bool assignmentChanged,
            bool stateChanged)
        {
            SDTask currentSnapshot = (SDTask)task.Clone();
            SDTask? previousSnapshot = previousTask is null ? null : (SDTask)previousTask.Clone();

            _taskEventSubject.Notify(new TaskSavedDomainEvent(currentSnapshot, previousSnapshot, currentUser));

            if (assignmentChanged)
            {
                _taskEventSubject.Notify(new TaskAssignedDomainEvent(currentSnapshot, previousSnapshot, currentUser));
            }

            if (stateChanged && previousSnapshot is not null)
            {
                _taskEventSubject.Notify(new TaskStatusChangedDomainEvent(currentSnapshot, previousSnapshot, currentUser));
            }
        }
    }
}
