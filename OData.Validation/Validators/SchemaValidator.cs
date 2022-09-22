using GillSoft.XmlComparer;
using GillSoft.XmlComparer.ConsoleApp;
using Microsoft.OData.Edm.Validation;
using System.Text;
using OData.Schema.Validation.Models;
using OData.Schema.Validation.Utils.XmlComparer;

namespace OData.Schema.Validation.Utils
{
    public class SchemaValidator
    {
        Dictionary<string, ModelContainer> SourceSchemas;
        Dictionary<string, ModelContainer> DestinationSchemas;
        Comparer Comparer;
        ComparisonReport ComparisonReport;
        public IEnumerable<EdmError> ValidationErrors;
        public Logger Logger;

        public SchemaValidator(Dictionary<string, ModelContainer> sourceSchemas, Dictionary<string, ModelContainer> destinationSchemas, Logger logger)
        {
            SourceSchemas = sourceSchemas;
            DestinationSchemas = destinationSchemas;
            Comparer = new Comparer(new TestXmlCompareHandler());
            ComparisonReport = new ComparisonReport();
            ValidationErrors = Enumerable.Empty<EdmError>();
            Logger = logger;
        }

        public void RunValidation()
        {
            ValidationErrors = ValdateSchema(SourceSchemas);
            foreach (var error in ValidationErrors)
            {
                var logEntry = new LogEntry(LogLevel.Error, error.ErrorMessage, "OdataError", error.ErrorLocation.ToString());
                Logger.Log(logEntry);
            }

            ValidateBreakingChanges();
        }

        public void ValidateBreakingChanges()
        {
            CompareCsdl();
            foreach (var deletion in ComparisonReport.Removals)
            {
                var errorMessage = "Deleted " + deletion.XPath;
                var logEntry = new LogEntry(LogLevel.Warning, errorMessage, "BreakingChange.Deletion", deletion.LineNumber.ToString(), deletion.XPath, deletion.Element);
                Logger.Log(logEntry);
            }

        }

        public void CompareCsdl()
        {
            HashSet<string> keySet = new HashSet<string>(DestinationSchemas.Keys);

            foreach (var sourceSchema in SourceSchemas)
            {
                if (DestinationSchemas.TryGetValue(sourceSchema.Key, out var destinationSchema))
                {
                    keySet.Remove(sourceSchema.Key);
                    ComparisonReport = Comparer.Compare(StringToStream(sourceSchema.Value.Csdl), StringToStream(destinationSchema.Csdl));
                }
            }

            string emptyCsdl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<edmx:Edmx xmlns:edmx=\"http://docs.oasis-open.org/odata/ns/edmx\" Version=\"4.0\"></edmx:Edmx>";
            foreach (var key in keySet)
            {
                if (DestinationSchemas.TryGetValue(key, out var destinationSchema))
                {
                    ComparisonReport = Comparer.Compare(StringToStream(emptyCsdl), StringToStream(destinationSchema.Csdl));
                }
            }
        }

        public static Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }
        public IEnumerable<EdmError> ValdateSchema(Dictionary<string, ModelContainer> schemas)
        {
            List<EdmError> validationErrorList = new List<EdmError>();

            foreach (var schema in schemas)
            {
                if (schema.Value != null)
                {
                    var ruleset = ValidationRuleSet.GetEdmModelRuleSet(new Version(4, 0));
                    IEnumerable<EdmError> validationErrors;
                    _ = schema.Value.IedmModel.Validate(ruleset, out validationErrors);
                    validationErrorList.AddRange(validationErrors);
                }
            }

            return validationErrorList;
        }

    }
}
