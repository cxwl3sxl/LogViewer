using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;

namespace LogViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly LogViewModel _logViewModel = new LogViewModel();
        readonly UdpServer _udpServer = new UdpServer();
        readonly List<LogEntity> _allLogs = new List<LogEntity>();
        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += delegate (object sender, EventArgs e)//执行拖拽
            {
                this._HwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            };
            DataContext = _logViewModel;
            _logViewModel.FilterChanged += _logViewModel_FilterChanged;
            _udpServer.LogReceived += _udpServer_LogReceived;
            _udpServer.ServerStarted += _udpServer_ServerStarted;
            _udpServer.ServerStoped += _udpServer_ServerStoped;
        }

        private void _logViewModel_FilterChanged()
        {
            var result = _allLogs.Where(d =>
                (d.App == _logViewModel.CurrentApp || _logViewModel.CurrentApp == LogViewModel.All) &&
                (d.Level == _logViewModel.CurrentLevel || _logViewModel.CurrentLevel == LogViewModel.All) &&
                (d.Logger == _logViewModel.CurrentLogger || _logViewModel.CurrentLogger == LogViewModel.All) &&
                (d.Thread == _logViewModel.CurrentThread || _logViewModel.CurrentThread == LogViewModel.All));
            RichTextBoxLogs.Document.Blocks.Clear();
            foreach (var entity in result)
            {
                ShowLogItem(entity);
            }
        }

        private void _udpServer_ServerStoped()
        {
            Dispatcher.Invoke(new Action<string>(ShowLog), "服务已经停止.");
        }

        private void _udpServer_ServerStarted()
        {
            Dispatcher.Invoke(new Action<string>(ShowLog), "服务已经成功启动.");
        }

        private void _udpServer_LogReceived(string obj)
        {
            if (!_logViewModel.IsWorking) return;
            try
            {
                var xml = new XmlDocument();
                xml.LoadXml(HttpUtility.HtmlDecode(obj));
                var log = xml.SelectSingleNode("log");
                if (log == null) return;
                var logObj = new LogEntity();
                logObj.Level = log.Attributes?["level"]?.Value;
                logObj.App = log.Attributes?["app"]?.Value;
                logObj.Content = log.InnerText;
                logObj.Logger = log.Attributes?["logger"]?.Value;
                logObj.Thread = log.Attributes?["thread"]?.Value;
                logObj.Time = log.Attributes?["time"]?.Value;
                Dispatcher.Invoke(new Action<LogEntity>(ShowLog), logObj);
            }
            catch
            {

            }
        }

        void ShowLog(string log)
        {
            RichTextBoxLogs.Document.Blocks.Add(new Paragraph(new Run(log)));
            if (_logViewModel.IsAutoScrollToEnd)
                RichTextBoxLogs.ScrollToEnd();
        }

        void ShowLog(LogEntity log)
        {
            if (log == null) return;
            _allLogs.Add(log);

            if (!_logViewModel.ApplicationNames.Contains(log.App))
            {
                _logViewModel.ApplicationNames.Add(log.App);
            }
            if (!_logViewModel.ThreadIds.Contains(log.Thread))
            {
                _logViewModel.ThreadIds.Add(log.Thread);
            }
            if (!_logViewModel.Loggers.Contains(log.Logger))
            {
                _logViewModel.Loggers.Add(log.Logger);
            }
            if (_logViewModel.CurrentApp != LogViewModel.All && _logViewModel.CurrentApp != log.App) return;
            if (_logViewModel.CurrentLogger != LogViewModel.All && _logViewModel.CurrentLogger != log.Logger) return;
            if (_logViewModel.CurrentThread != LogViewModel.All && _logViewModel.CurrentThread != log.Thread) return;
            if (_logViewModel.CurrentLevel != LogViewModel.All && _logViewModel.CurrentLevel != log.Level) return;
            ShowLogItem(log);
        }

        void ShowLogItem(LogEntity log)
        {
            var line = new Run($"[{log.Time}] [{log.Logger}] [{log.Thread}] {log.Content}");
            switch (log.Level)
            {
                case "FATAL":
                    line.Foreground = new SolidColorBrush(Colors.Red);
                    line.Background = new SolidColorBrush(Colors.Black);
                    break;
                case "ERROR":
                    line.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case "WARN":
                    line.Foreground = new SolidColorBrush(Colors.Orange);
                    break;
                case "INFO":
                    line.Foreground = new SolidColorBrush(Colors.DodgerBlue);
                    break;
                case "DEBUG":
                    line.Foreground = new SolidColorBrush(Colors.DarkGray);
                    break;
            }
            RichTextBoxLogs.Document.Blocks.Add(new Paragraph(line));

            if (_logViewModel.IsAutoScrollToEnd)
                RichTextBoxLogs.ScrollToEnd();
        }

        private void CleanLogs_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要清空所有日志么？", "询问", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.Yes)
            {
                RichTextBoxLogs.Document.Blocks.Clear();
                _allLogs.Clear();
            }
        }

        private void CopySetting_OnClick(object sender, RoutedEventArgs e)
        {
            var text = "";
            try
            {
                var app = new AppNameInput { Owner = this };
                var dialogReasult = app.ShowDialog();
                if (dialogReasult.HasValue && dialogReasult.Value)
                {
                    text = Properties.Resources.UdpAppender
                        .Replace("#PORT#", _logViewModel.Port.ToString())
                        .Replace("#APPNAME#", app.TextBoxAppName.Text.Replace("\"", "").Replace("<", "").Replace(">", "").Replace("&", ""));
                    Clipboard.SetDataObject(text);
                    MessageBox.Show("复制成功，请将内容粘贴到目标程序的配置文件中！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败，请手动复制！\n{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                var copy = new CopyContent
                {
                    Owner = this,
                    TextBoxContent = { Text = text }
                };
                copy.TextBoxContent.SelectionStart = 0;
                copy.TextBoxContent.SelectAll();
                copy.ShowDialog();
            }
        }

        private void StartOrStop_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_logViewModel.IsWorking)
                {
                    _udpServer.Start(_logViewModel.Port);
                }
                else
                {
                    if (MessageBox.Show("确定要停止服务么？", "询问", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                        MessageBoxResult.Yes)
                    {
                        _udpServer.Stop();
                    }
                    else
                    {
                        _logViewModel.IsWorking = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作失败:" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                _logViewModel.IsWorking = !_logViewModel.IsWorking;
            }
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private HwndSource _HwndSource;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private void ResizePressed(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                ResizeWindow();
        }

        private void ResizeWindow()
        {
            SendMessage(_HwndSource.Handle, 0x112, (IntPtr)(61440 + 8), IntPtr.Zero);
        }

        private void ShutDown_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ButtonRestore_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            ButtonMax.Visibility = Visibility.Visible;
            ButtonRestore.Visibility = Visibility.Collapsed;
            BorderThickness = new Thickness(24);
        }

        private void ButtonMin_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonMax_OnClick(object sender, RoutedEventArgs e)
        {
            BorderThickness = new Thickness(5);
            WindowState = WindowState.Maximized;
            ButtonMax.Visibility = Visibility.Collapsed;
            ButtonRestore.Visibility = Visibility.Visible;
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Log4Net日志收集器\nhttps://github.com/cxwl3sxl/LogViewer", "关于");
        }
    }
}
