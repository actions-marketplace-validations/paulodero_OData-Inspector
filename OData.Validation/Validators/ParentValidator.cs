using GillSoft.XmlComparer;
using GillSoft.XmlComparer.ConsoleApp;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;
using System.Runtime.Serialization.Formatters.Binary;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Xml;
using Microsoft.OData.Edm.Csdl;
using System.Reflection;
using System.IO;
using OData.Schema.Validation.Models;
using OData.Schema.Validation.Utils.XmlComparer;

namespace OData.Schema.Validation.Utils
{
    public class ParentValidator
    {
        Dictionary<string, ModelContainer> SourceSchemas;
        Dictionary<string, ModelContainer> DestinationSchemas;
        IEnumerable<EdmError> validationErrors;
        Comparer Comparer;
        ComparisonReport ComparisonReport;

        public ParentValidator(Dictionary<string, ModelContainer> sourceSchemas, Dictionary<string, ModelContainer> destinationSchemas)
        {
            SourceSchemas = sourceSchemas;
            DestinationSchemas = destinationSchemas;
            Comparer = new Comparer(new TestXmlCompareHandler());
            ComparisonReport = new ComparisonReport();
        }

        public void RunValidateion()
        {
            validationErrors = ValdateSchema(DestinationSchemas);
            CompareCsdl();
        }

        private MemoryStream ToStream(IEdmModel model)
        {
            var stream = new MemoryStream();
            using (var writer = XmlWriter.Create(stream))
            {
                IEnumerable<EdmError> errors;
                CsdlWriter.TryWriteCsdl(model, writer, CsdlTarget.OData, out errors);
            }

            return stream;
        }

        public void CompareCsdl()
        {
            HashSet<string> keySet = new HashSet<string>(DestinationSchemas.Keys);

            foreach (var sourceSchema in SourceSchemas)
            {
                if (DestinationSchemas.TryGetValue(sourceSchema.Key, out var destinationSchema))
                {
                    ComparisonReport = Comparer.Compare(StringToStream(sourceSchema.Value.Csdl), StringToStream(destinationSchema.Csdl));
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
