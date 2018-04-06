namespace LogViewer
{
    public class LogEntity
    {
        public string Level { get; set; }
        public string App { get; set; }
        public string Thread { get; set; }
        public string Time { get; set; }
        public string Logger { get; set; }
        public string Content { get; set; }
    }
}
