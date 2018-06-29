using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using LogViewer.Annotations;

namespace LogViewer
{
    public class LogViewModel : INotifyPropertyChanged
    {
        public const string All = "ALL";
        public readonly ThreadInfo AllThreadInfo;
        public readonly LogNameInfo AllLogName;
        public LogViewModel()
        {
            ApplicationNames = new ObservableCollection<string> { All };
            AllThreadInfo = new ThreadInfo()
            {
                IsChecked = true,
                ThreadId = All,
                AppName = All
            };
            AllThreadInfo.PropertyChanged += AllThreadInfo_PropertyChanged;
            ThreadIds = new ObservableCollection<ThreadInfo> { AllThreadInfo };
            Loggers = new ObservableCollection<LogNameInfo>();
            LoggerLevels = new ObservableCollection<string>
            {
                All,
                "FATAL",
                "ERROR",
                "WARN",
                "INFO",
                "DEBUG"
            };
            AllLogName = new LogNameInfo()
            {
                IsChecked = true,
                Name = All,
                AppName = All
            };
            AllLogName.PropertyChanged += AllLogName_PropertyChanged;
            Loggers.Add(AllLogName);
            CurrentApp = All;
            CurrentLogger = AllLogName;
            CurrentThread = AllThreadInfo;
            CurrentLevel = All;
        }

        private void AllLogName_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is LogNameInfo all)) return;
            if (e.PropertyName != nameof(LogNameInfo.IsChecked)) return;
            foreach (var logger in Loggers)
            {
                logger.IsChecked = all.IsChecked;
            }
        }

        private void AllThreadInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is ThreadInfo all)) return;
            if (e.PropertyName != nameof(ThreadInfo.IsChecked)) return;
            foreach (var threadInfo in ThreadIds)
            {
                threadInfo.IsChecked = all.IsChecked;
            }
        }

        public ObservableCollection<string> ApplicationNames { get; }
        public ObservableCollection<ThreadInfo> ThreadIds { get; }
        public ObservableCollection<LogNameInfo> Loggers { get; }
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

        public ThreadInfo CurrentThread
        {
            get => _currentThread;
            set
            {
                var changed = _currentThread != value;
                _currentThread = value;
                if (changed) FilterChanged?.Invoke();
            }
        }

        public LogNameInfo CurrentLogger
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
        private ThreadInfo _currentThread;
        private LogNameInfo _currentLogger;
        private string _currentLevel;
        private int _fatal;
        private int _error;
        private int _warn;
        private int _info;
        private int _debug;
        private int _total;

        public int Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        public int Fatal
        {
            get => _fatal;
            set
            {
                _fatal = value;
                OnPropertyChanged(nameof(Fatal));
            }
        }

        public int Error
        {
            get => _error;
            set
            {
                _error = value;
                OnPropertyChanged(nameof(Error));
            }
        }

        public int Warn
        {
            get => _warn;
            set
            {
                _warn = value;
                OnPropertyChanged(nameof(Warn));
            }
        }

        public int Info
        {
            get => _info;
            set
            {
                _info = value;
                OnPropertyChanged(nameof(Info));
            }
        }

        public int Debug
        {
            get => _debug;
            set
            {
                _debug = value;
                OnPropertyChanged(nameof(Debug));
            }
        }

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
        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LogCategoryInfo : INotifyPropertyChanged
    {
        private string _appName;
        private bool _isChecked;

        public string AppName
        {
            get => _appName;
            set
            {
                var old = _appName;
                _appName = value;
                if (_appName != old) OnPropertyChanged(nameof(AppName));
            }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                var old = _isChecked;
                _isChecked = value;
                if (_isChecked != old) OnPropertyChanged(nameof(IsChecked));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThreadInfo : LogCategoryInfo
    {
        public string ThreadId { get; set; }
    }

    public class LogNameInfo : LogCategoryInfo
    {
        public string Name { get; set; }
    }
}