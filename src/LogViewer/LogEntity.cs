namespace LogViewer
{
    public class LogEntity
    {
        private static int _innerLogId = 0;
        static readonly object SyncRoot = new object();

        public LogEntity()
        {
            lock (SyncRoot)
                LogId = _innerLogId++;
        }

        public int LogId { get; }
        public string Level { get; set; }
        public string App { get; set; }
        public string Thread { get; set; }
        public string Time { get; set; }
        public string Logger { get; set; }
        public string Content { get; set; }
    }
}
