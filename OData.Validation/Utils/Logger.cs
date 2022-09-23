namespace OData.Schema.Validation.Utils
{
    public class Logger
    {
        public List<LogEntry> LogEntries = new();

        public void Log(LogEntry logEntry)
        {
            LogEntries.Add(logEntry);
        }
    }
}
