using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
