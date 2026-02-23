using System.ComponentModel;

namespace SDNet.Pages
{
    public partial class TaskListPage : ContentPage
    {
        private readonly TaskListPageModel _model;

        public TaskListPage(TaskListPageModel model)
        {
            InitializeComponent();
            _model = model;
            BindingContext = model;
            _model.PropertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TaskListPageModel.FocusTaskId))
            {
                return;
            }

            if (!_model.FocusTaskId.HasValue)
            {
                return;
            }

            var target = _model.FilteredTasks.FirstOrDefault(t => t.Id == _model.FocusTaskId.Value);
            if (target is null)
            {
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TasksCollectionView.ScrollTo(target, position: ScrollToPosition.End, animate: true);
            });
        }
    }
}
