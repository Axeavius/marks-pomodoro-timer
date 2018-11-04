using MarksPomodoroTimer.ViewModels;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MarksPomodoroTimer
{
    public sealed partial class MainPage : Page
    {
        DispatcherTimer PomodoroTimer = new DispatcherTimer();
        DispatcherTimer BreakTimeTimer = new DispatcherTimer();
        DispatcherTimer Clock = new DispatcherTimer();

        MediaPlayer taskCompleteSound = new MediaPlayer();
        MediaPlayer breakCompleteSound = new MediaPlayer();

        int timerDefaultLength = 25 * 60;
        int timerRemainingTime = 25 * 60;
        TimeSpan timerTimeSpan;

        int breakTimeDefaultLength = 5 * 60;
        int breakTimeRemainingTime = 5 * 60;
        int breakTimeLongBreakDefaultLength = 15 * 60;
        TimeSpan breakTimeTimeSpan;

        int selectedTask;
        int taskListCount;
        int completedTasks = 0;

        bool breakTime = false;
        bool overlayVisible = false;

        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = new TaskListViewModel();

            taskCompleteSound.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Audio/TaskComplete.wav", UriKind.RelativeOrAbsolute));
            breakCompleteSound.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Audio/BreakComplete.wav", UriKind.RelativeOrAbsolute));

            PomodoroTimerSetup();
            TaskListSetup();
            ClockSetup();
        }

        public TaskListViewModel ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Window.Current.SetTitleBar(AppTitleBar);
        }


        // Pomodoro Timer
        private void PomodoroTimerSetup()
        {
            PomodoroTimer = new DispatcherTimer();
            PomodoroTimer.Tick += PomodoroTimer_Tick;
            PomodoroTimer.Interval = new TimeSpan(0, 0, 1);

            TimerRadial.Maximum = timerRemainingTime;
            TimerRadial.Value = timerRemainingTime;
        }

        private void PomodoroTimer_Tick(object sender, object e)
        {
            TimerRadial.Value = timerRemainingTime;
            timerTimeSpan = TimeSpan.FromSeconds(timerRemainingTime);
            TimerText.Text = timerTimeSpan.ToString(@"m\:ss");

            if (timerRemainingTime > 0)
            {
                timerRemainingTime--;

                UpdateTimerHeader();
                PlayPauseTimerButton.IsChecked = true;
            }
            else
            {
                PomodoroTimer.Stop();
                completedTasks++;
                breakTime = true;
                PlayPauseTimerButton.IsChecked = false;

                // TODO: Check checkbox of currently selected task

                taskCompleteSound.Play();

                DisplayBreakTimeOverlay.Begin();
                overlayVisible = true;
                StartBreakTimeButton.IsHitTestVisible = true;

                TimerText.Text = "Time's Up!";
                UpdateTimerHeader();
            }
        }

        private void BreakTimeSetup()
        {
            BreakTimeTimer = new DispatcherTimer();
            BreakTimeTimer.Tick += BreakTime_Tick;
            BreakTimeTimer.Interval = new TimeSpan(0, 0, 1);

            if (completedTasks % 4 == 0)
            {
                breakTimeRemainingTime = breakTimeLongBreakDefaultLength;
            }

            TimerRadial.Maximum = breakTimeRemainingTime;
            TimerRadial.Value = breakTimeRemainingTime;
        }

        private void BreakTime_Tick(object sender, object e)
        {
            TimerRadial.Value = breakTimeRemainingTime;
            breakTimeTimeSpan = TimeSpan.FromSeconds(breakTimeRemainingTime);
            TimerText.Text = breakTimeTimeSpan.ToString(@"m\:ss");

            if (breakTimeRemainingTime > 0)
            {
                breakTimeRemainingTime--;

                UpdateTimerHeader();
                PlayPauseTimerButton.IsChecked = true;
            }
            else
            {
                BreakTimeTimer.Stop();
                breakTime = false;
                PlayPauseTimerButton.IsChecked = false;

                UpdateTimerHeader();
                SelectNextTask();

                breakCompleteSound.Play();

                DisplayTimerOverlay.Begin();
                overlayVisible = true;
                StartNextTaskButton.IsHitTestVisible = true;

                TimerText.Text = "Break Over!";
                UpdateTimerHeader();
            }
        }

        private void ResetTimer()
        {
            PomodoroTimer.Stop();
            BreakTimeTimer.Stop();

            if (breakTime)
            {
                if (timerRemainingTime <= timerDefaultLength && breakTimeRemainingTime != breakTimeDefaultLength)
                {
                    TimerRadial.Value = breakTimeDefaultLength;
                    breakTimeRemainingTime = breakTimeDefaultLength;

                    BreakTimeSetup();
                }
            }
            else
            {
                if (breakTimeRemainingTime <= breakTimeDefaultLength && timerRemainingTime != timerDefaultLength)
                {
                    TimerRadial.Value = timerDefaultLength;
                    timerRemainingTime = timerDefaultLength;

                    PomodoroTimerSetup();
                }
            }

            PlayPauseTimerButton.IsChecked = false;
            TimerText.Text = "Ready?";
        }

        private void UpdateTimerHeader()
        {
            TaskListSetup();

            if (breakTime)
            {
                TimerHeader.Text = "Break Time";
            }
            else
            {
                TimerHeader.Text = "Task " + selectedTask;
            }
        }


        // Task List
        private void TaskListSetup()
        {
            selectedTask = System.Convert.ToInt32(TaskList.SelectedIndex + 1);
            taskListCount = ViewModel.Tasks.Count;
        }

        private void SelectNextTask()
        {
            TaskListSetup();

            if (selectedTask == taskListCount)
            {
                TaskList.SelectedItem = ViewModel.Tasks[0];
            }
            else
            {
                TaskList.SelectedItem = ViewModel.Tasks[TaskList.SelectedIndex + 1];
            }
        }


        // Clock
        private void ClockSetup()
        {
            Clock.Tick += Clock_Tick;
            Clock.Interval = new TimeSpan(0, 0, 1);
            Clock.Start();
        }

        private void Clock_Tick(object sender, object e)
        {
            ClockTime.Text = DateTime.Now.ToString("h:mm tt");
        }


        // Click Events
        private void TaskList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (overlayVisible)
            {
                if (timerRemainingTime == 0 && BreakTimeTimer.IsEnabled == false)
                {
                    HideBreakTimeOverlay.Begin();
                }
                if (breakTimeRemainingTime == 0 && PomodoroTimer.IsEnabled == false)
                {
                    HideTimerOverlay.Begin();
                }

                overlayVisible = false;
            }

            PomodoroTimer.Stop();
            BreakTimeTimer.Stop();

            breakTime = false;
            TaskListSetup();
            PomodoroTimerSetup();
            ResetTimer();
            UpdateTimerHeader();
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Tasks.Add("Task " + (ViewModel.Tasks.Count + 1));
            TaskListSetup();
        }

        private void StartPauseTimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (breakTime)
            {
                if (BreakTimeTimer.IsEnabled)
                {
                    BreakTimeTimer.Stop();
                }
                else
                {
                    if (overlayVisible)
                    {
                        BreakTimeSetup();
                        ResetTimer();

                        HideBreakTimeOverlay.Begin();
                        overlayVisible = false;
                        StartBreakTimeButton.IsHitTestVisible = false;

                        PlayPauseTimerButton.IsChecked = true;
                    }

                    TimerText.Text = "Go!";

                    BreakTimeTimer.Start();
                }
            }
            else
            {
                if (PomodoroTimer.IsEnabled)
                {
                    PomodoroTimer.Stop();
                }
                else
                {
                    if (overlayVisible)
                    {
                        PomodoroTimerSetup();
                        ResetTimer();

                        HideTimerOverlay.Begin();
                        overlayVisible = false;

                        PlayPauseTimerButton.IsChecked = true;
                        StartNextTaskButton.IsHitTestVisible = false;
                    }

                    TimerText.Text = "Go!";

                    PomodoroTimer.Start();
                }
            }
        }

        private void NextTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (breakTime)
            {
                if (overlayVisible)
                {
                    HideBreakTimeOverlay.Begin();
                    overlayVisible = false;
                    StartBreakTimeButton.IsHitTestVisible = false;
                }

                BreakTimeTimer.Stop();
                SelectNextTask();

                breakTime = false;
            }
            else
            {
                if (overlayVisible)
                {
                    HideTimerOverlay.Begin();
                    overlayVisible = false;
                    StartNextTaskButton.IsHitTestVisible = false;
                }
                else
                {
                    PomodoroTimer.Stop();
                    SelectNextTask();
                }
            }

            PomodoroTimerSetup();
            ResetTimer();

            TimerText.Text = "Ready?";
        }

        private void ResetTimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!overlayVisible)
            {
                ResetTimer();
            }
        }

        private void StartBreakTimeButton_Click(object sender, RoutedEventArgs e)
        {
            HideBreakTimeOverlay.Begin();
            overlayVisible = false;

            StartBreakTimeButton.IsHitTestVisible = false;
            PlayPauseTimerButton.IsChecked = true;

            BreakTimeSetup();
            ResetTimer();
            BreakTimeTimer.Start();

            TimerText.Text = "Go!";
        }

        private void StartNextTaskButton_Click(object sender, RoutedEventArgs e)
        {
            HideTimerOverlay.Begin();
            overlayVisible = false;

            StartNextTaskButton.IsHitTestVisible = false;
            PlayPauseTimerButton.IsChecked = true;

            PomodoroTimerSetup();
            ResetTimer();
            PomodoroTimer.Start();

            TimerText.Text = "Go!";
        }
    }
}
