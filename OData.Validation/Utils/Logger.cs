namespace OData.Schema.Validation.Utils
{
    public class Logger
    {
        public List<LogEntry> logEntries = new();

        public void Log(LogEntry logEntry)
        {
            logEntries.Add(logEntry);
        }
    }
}
