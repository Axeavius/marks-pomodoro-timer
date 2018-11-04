using System.Collections.ObjectModel;

namespace MarksPomodoroTimer.ViewModels
{
    public class TaskListViewModel
    {
        private ObservableCollection<string> tasks = new ObservableCollection<string>();
        public ObservableCollection<string> Tasks { get { return this.tasks; } }

        public TaskListViewModel()
        {
            this.tasks.Add("Task 1");
            this.tasks.Add("Task 2");
            this.tasks.Add("Task 3");
            this.tasks.Add("Task 4");
        }
    }
}
