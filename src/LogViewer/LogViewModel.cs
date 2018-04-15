using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using LogViewer.Annotations;

namespace LogViewer
{
    public class LogViewModel : INotifyPropertyChanged
    {
        public const string All = "=ALL=";
        public LogViewModel()
        {
            ApplicationNames = new ObservableCollection<string> { All };
            ThreadIds = new ObservableCollection<string> { All };
            Loggers = new ObservableCollection<string>();
            LoggerLevels = new ObservableCollection<string>
            {
                All,
                "FATAL",
                "ERROR",
                "WARN",
                "INFO",
                "DEBUG"
            };
            Loggers.Add(All);
            CurrentApp = All;
            CurrentLogger = All;
            CurrentThread = All;
            CurrentLevel = All;
        }
        public ObservableCollection<string> ApplicationNames { get; }
        public ObservableCollection<string> ThreadIds { get; }
        public ObservableCollection<string> Loggers { get; }
        public ObservableCollection<string> LoggerLevels { get; }

        public event Action FilterChanged;

        public string CurrentApp
        {
            get => _currentApp;
            set
            {
                var changed = _currentApp != value;
                _currentApp = value;
                if (changed) FilterChanged?.Invoke();
            }
        }

        public string CurrentThread
        {
            get => _currentThread;
            set
            {
                var changed = _currentThread != value;
                _currentThread = value;
                if (changed) FilterChanged?.Invoke();
            }
        }

        public string CurrentLogger
        {
            get => _currentLogger;
            set
            {
                var changed = _currentLogger != value;
                _currentLogger = value;
                if (changed) FilterChanged?.Invoke();
            }
        }

        public string CurrentLevel
        {
            get => _currentLevel;
            set
            {
                var changed = _currentLevel != value;
                _currentLevel = value;
                if (changed) FilterChanged?.Invoke();
            }
        }

        private bool _isAutoScrollToEnd = true;
        public bool IsAutoScrollToEnd
        {
            get => _isAutoScrollToEnd;
            set
            {
                _isAutoScrollToEnd = value;
                OnPropertyChanged(nameof(IsAutoScrollToEnd));
            }
        }

        private bool _isWorking = false;
        public bool IsWorking
        {
            get => _isWorking;
            set
            {
                _isWorking = value;
                OnPropertyChanged(nameof(IsWorking));
                OnPropertyChanged(nameof(CanChangePort));
            }
        }

        private int _port = 7171;
        private string _currentApp;
        private string _currentThread;
        private string _currentLogger;
        private string _currentLevel;

        public int Port
        {
            get => _port;
            set
            {
                if (value < 7000 || value > 65530) return;
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        public bool CanChangePort => !IsWorking;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}