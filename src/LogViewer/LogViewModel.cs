using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using LogViewer.Annotations;

namespace LogViewer
{
    public class LogViewModel : INotifyPropertyChanged
    {
        public const string All = "ALL";
        public readonly ThreadInfo AllThreadInfo;
        public readonly LogNameInfo AllLogName;
        public readonly AppInfo AllAppInfo;

        public LogViewModel()
        {
            AllAppInfo = new AppInfo()
            {
                IsChecked = true,
                AppName = All
            };
            AllAppInfo.PropertyChanged += AllAppInfo_PropertyChanged;
            ApplicationNames = new ObservableCollection<AppInfo> { AllAppInfo };
            AllThreadInfo = new ThreadInfo()
            {
                IsChecked = true,
                ThreadId = All,
                AppName = All
            };
            AllThreadInfo.PropertyChanged += AllThreadInfo_PropertyChanged;
            ThreadIds = new SortableObservableCollection<ThreadInfo> { AllThreadInfo };
            ThreadIds.SortingSelector = a => $"{a.AppName}{a.ThreadId}";
            ThreadIds.Descending = false;

            Loggers = new SortableObservableCollection<LogNameInfo>();

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
            Loggers.SortingSelector = a => $"{a.AppName}{a.Name}";
            Loggers.Descending = false;

            CurrentApp = AllAppInfo;
            CurrentLogger = AllLogName;
            CurrentThread = AllThreadInfo;
            CurrentLevel = All;
        }

        private void AllAppInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is AppInfo all)) return;
            if (e.PropertyName != nameof(AppInfo.IsChecked)) return;
            foreach (var app in ApplicationNames)
            {
                app.IsChecked = all.IsChecked;
            }
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

        public ObservableCollection<AppInfo> ApplicationNames { get; }
        public SortableObservableCollection<ThreadInfo> ThreadIds { get; }
        public SortableObservableCollection<LogNameInfo> Loggers { get; }
        public ObservableCollection<string> LoggerLevels { get; }

        public event Action FilterChanged;

        public AppInfo CurrentApp
        {
            get => _currentApp;
            set
            {
                var changed = _currentApp != value;
                _currentApp = value;
                if (!changed) return;
                var checkedApp = ApplicationNames.Where(a => a.IsChecked).ToArray();
                foreach (var thread in ThreadIds)
                {
                    if (thread.AppName == AllThreadInfo.AppName) continue;
                    thread.IsHide = checkedApp.FirstOrDefault(a => a.AppName == thread.AppName) == null;
                    if (thread.IsHide) thread.IsChecked = false;
                }

                foreach (var logger in Loggers)
                {
                    if (logger.AppName == AllLogName.AppName) continue;
                    logger.IsHide = checkedApp.FirstOrDefault(a => a.AppName == logger.AppName) == null;
                    if (logger.IsHide) logger.IsChecked = false;
                }

                FilterChanged?.Invoke();
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
        private AppInfo _currentApp;
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
        private bool _isHide;

        public string AppName
        {
            get => _appName;
            set
            {
                if (value == _appName) return;
                _appName = value;
                OnPropertyChanged(nameof(AppName));
            }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public bool IsHide
        {
            get => _isHide;
            set
            {
                if (value == _isHide) return;
                _isHide = value;
                OnPropertyChanged(nameof(IsHide));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"A:{AppName}-C:{(IsChecked ? "Y" : "N")}-H:{(IsHide ? "Y" : "N")}";
        }
    }

    public class ThreadInfo : LogCategoryInfo
    {
        public string ThreadId { get; set; }

        public override string ToString()
        {
            return $"T:{ThreadId}-{base.ToString()}";
        }
    }

    public class LogNameInfo : LogCategoryInfo
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"L:{Name}-{base.ToString()}";
        }
    }

    public class AppInfo : LogCategoryInfo
    {

    }
}