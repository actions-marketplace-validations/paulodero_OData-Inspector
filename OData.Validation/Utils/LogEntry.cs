using System.Xml.Linq;

namespace OData.Schema.Validation.Utils
{
    public class LogEntry
    {
        public LogLevel LogLevel { get; set; }

        public string Path { get; set; }
        public string Message { get; set; }
        public string EntryType { get; set; }
        public string Location { get; set; }
        public XElement Element { get; set; }

        public LogEntry(LogLevel logLevel, string message = "", string entryType = "", string location = "", string path = "", XElement element = null) 
        {
            LogLevel = logLevel;
            Message = message;
            EntryType = entryType;
            Location = location;
            Path = path;
            Element = element;
        }
    }

}
