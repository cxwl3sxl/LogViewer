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
            ApplicationNames = new ObservableCollection<string>();
            ApplicationNames.Add(All);
            ThreadIds = new ObservableCollection<string>();
            ThreadIds.Add(All);
            Loggers = new ObservableCollection<string>();
            Loggers.Add(All);
            CurrentApp = All;
            CurrentLogger = All;
            CurrentThread = All;
        }
        public ObservableCollection<string> ApplicationNames { get; private set; }
        public ObservableCollection<string> ThreadIds { get; private set; }
        public ObservableCollection<string> Loggers { get; private set; }

        public string CurrentApp { get; set; }
        public string CurrentThread { get; set; }
        public string CurrentLogger { get; set; }

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