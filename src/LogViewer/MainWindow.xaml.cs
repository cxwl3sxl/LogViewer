using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
    public partial class MainWindow
    {
        private readonly int MaxLinePrePage = 1000;
        private int _lastLogId;
        readonly LogViewModel _logViewModel = new LogViewModel();
        readonly UdpServer _udpServer = new UdpServer();
        readonly List<LogEntity> _allLogs = new List<LogEntity>();
        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += delegate (object sender, EventArgs e)//执行拖拽
            {
                _hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
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
            if (string.IsNullOrWhiteSpace(obj)) return;
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
                // ignored
            }
        }

        void ShowLog(string log)
        {
            AppendContent(new Paragraph(new Run(log)));
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
            _logViewModel.Total++;
            switch (log.Level)
            {
                case "FATAL":
                    _logViewModel.Fatal++;
                    break;
                case "ERROR":
                    _logViewModel.Error++;
                    break;
                case "WARN":
                    _logViewModel.Warn++;
                    break;
                case "INFO":
                    _logViewModel.Info++;
                    break;
                case "DEBUG":
                    _logViewModel.Debug++;
                    break;
            }
            if (_logViewModel.CurrentApp != LogViewModel.All && _logViewModel.CurrentApp != log.App) return;
            if (_logViewModel.CurrentLogger != LogViewModel.All && _logViewModel.CurrentLogger != log.Logger) return;
            if (_logViewModel.CurrentThread != LogViewModel.All && _logViewModel.CurrentThread != log.Thread) return;
            if (_logViewModel.CurrentLevel != LogViewModel.All && _logViewModel.CurrentLevel != log.Level) return;

            if (_lastLogId != 0 && log.LogId > _lastLogId) return;
            ShowLogItem(log);
        }

        void ShowLogItem(LogEntity log)
        {
            AppendContent(GetParagraphForLog(log));
            if (_logViewModel.IsAutoScrollToEnd)
                RichTextBoxLogs.ScrollToEnd();
        }

        Paragraph GetParagraphForLog(LogEntity log)
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
                    line.Foreground = new SolidColorBrush(Colors.Gray);
                    break;
            }
            return new Paragraph(line);
        }

        private void AppendContent(Paragraph paragraph)
        {
            if (RichTextBoxLogs.Document.Blocks.Count >= MaxLinePrePage)
            {
                var first = RichTextBoxLogs.Document.Blocks.ElementAtOrDefault(0);
                if (first != null)
                    RichTextBoxLogs.Document.Blocks.Remove(first);
            }
            RichTextBoxLogs.Document.Blocks.Add(paragraph);
        }

        private void CleanLogs_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要清空所有日志么？", "询问", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.Yes)
            {
                RichTextBoxLogs.Document.Blocks.Clear();
                _allLogs.Clear();
                _logViewModel.Total = 0;
                _logViewModel.Debug = 0;
                _logViewModel.Info = 0;
                _logViewModel.Warn = 0;
                _logViewModel.Error = 0;
                _logViewModel.Fatal = 0;
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

        private HwndSource _hwndSource;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private void ResizePressed(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                ResizeWindow();
        }

        private void ResizeWindow()
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + 8), IntPtr.Zero);
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

        private void FindClose_OnClick(object sender, RoutedEventArgs e)
        {
            BorderFindBox.Visibility = Visibility.Collapsed;
            foreach (var range in _findResult)
            {
                range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.White));
            }
            _findResult.Clear();
            _logViewModel.IsAutoScrollToEnd = true;
            _currentIndex = 0;
        }

        private void Find_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                BorderFindBox.Visibility = Visibility.Visible;
                TextBoxKeyWords.Text = "";
                LabelResultCount.Content = $"0/0";
                TextBoxKeyWords.Focus();
            }
        }

        readonly List<TextRange> _findResult = new List<TextRange>();
        private int _currentIndex;
        private void TextBoxKeyWords_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoSearch();
            }
        }

        void DoSearch()
        {
            foreach (var range in _findResult)
            {
                range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.White));
            }
            _findResult.Clear();
            _currentIndex = 0;
            if (string.IsNullOrWhiteSpace(TextBoxKeyWords.Text)) return;
            var result = FindWordFromPosition(TextBoxKeyWords.Text);
            LabelResultCount.Content = $"{_currentIndex}/{result.Count}";
            _findResult.AddRange(result);
            _logViewModel.IsAutoScrollToEnd = false;
            if (_findResult.Count > 0)
                _findResult[0].ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
        }

        List<TextRange> FindWordFromPosition(string keyword)
        {
            var regex = new Regex(keyword);
            var result = new List<TextRange>();
            var position = RichTextBoxLogs.Document.ContentStart;
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    //拿出Run的Text        
                    var text = position.GetTextInRun(LogicalDirection.Forward);
                    //可能包含多个keyword,做遍历查找           
                    var all = regex.Matches(text);
                    foreach (Match match in all)
                    {
                        TextPointer start = position.GetPositionAtOffset(match.Index);
                        TextPointer end = start?.GetPositionAtOffset(match.Length);
                        var current = new TextRange(start, end);
                        result.Add(current);
                        current.ApplyPropertyValue(TextElement.BackgroundProperty,
                            new SolidColorBrush(Colors.GreenYellow));
                    }
                }
                //文字指针向前偏移   
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
            return result;
        }

        private void FindPre_OnClick(object sender, RoutedEventArgs e)
        {
            if (_findResult.Count == 0) return;
            _currentIndex--;
            if (_currentIndex < 0)
            {
                _currentIndex = _findResult.Count - 1;
            }
            FocusResult(_currentIndex, -1);
        }

        private void FindNext_OnClick(object sender, RoutedEventArgs e)
        {
            if (_findResult.Count == 0) return;
            _currentIndex++;
            if (_currentIndex >= _findResult.Count)
            {
                _currentIndex = 0;
            }
            FocusResult(_currentIndex, +1);
        }

        void FocusResult(int index, int action)
        {
            LabelResultCount.Content = $"{index}/{_findResult.Count}";
            var pre = index - action;
            if (pre < 0) pre = _findResult.Count - 1;
            if (pre >= _findResult.Count) pre = 0;
            _findResult[pre].ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.GreenYellow));
            if (index < 0) index = _findResult.Count - 1;
            if (index >= _findResult.Count) index = 0;
            _findResult[index].ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
            _findResult[index].Start?.Paragraph?.BringIntoView();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _logViewModel.IsWorking = true;
            StartOrStop_OnClick(null, null);
        }

        private void DoSearch_OnClick(object sender, RoutedEventArgs e)
        {
            DoSearch();
        }

        private void FirstPageLog_OnClick(object sender, RoutedEventArgs e)
        {
            _lastLogId = MaxLinePrePage;
            ReLoadLogs();
        }

        private void PrePageLog_OnClick(object sender, RoutedEventArgs e)
        {
            if (_lastLogId == 0)
            {
                _lastLogId = _allLogs.LastOrDefault()?.LogId ?? 0;
                ReLoadLogs();
            }
            else if (_lastLogId > MaxLinePrePage)
            {
                _lastLogId -= MaxLinePrePage;
                ReLoadLogs();
            }
        }

        private void NextPageLog_OnClick(object sender, RoutedEventArgs e)
        {
            _lastLogId += MaxLinePrePage;
            if (_lastLogId >= _allLogs.Count) _lastLogId = 0;
            ReLoadLogs();
        }

        private void LastPageLog_OnClick(object sender, RoutedEventArgs e)
        {
            _lastLogId = 0;
            ReLoadLogs();
        }

        void ReLoadLogs()
        {
            if (_lastLogId < MaxLinePrePage || _allLogs.Count < MaxLinePrePage) return;
            var logs = _lastLogId == 0 ? _allLogs.GetRange(_allLogs.Count - MaxLinePrePage, MaxLinePrePage) : _allLogs.Skip(_lastLogId - MaxLinePrePage).Take(MaxLinePrePage);
            RichTextBoxLogs.Document.Blocks.Clear();
            foreach (var log in logs)
            {
                RichTextBoxLogs.Document.Blocks.Add(GetParagraphForLog(log));
            }
        }
    }
}
