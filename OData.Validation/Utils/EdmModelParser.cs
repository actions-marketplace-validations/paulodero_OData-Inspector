using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Microsoft.OData.Edm;
using System.Text;
using System.Xml;

namespace OData.Schema.Validation.Utils
{
    public class EdmModelParser
    {
        public static IEdmModel ParseEdmModel(Stream stream)
        {
            var memStream = new MemoryStream();
            stream.CopyTo(memStream);
            memStream.Position = 0;

            if (!SchemaReader.TryParse(
                new XmlReader[] { XmlReader.Create(memStream) }, out IEdmModel model, out IEnumerable<EdmError> errors))
            {
                if (ParseModel(memStream, out IEdmModel iedmModel, out _))
                {
                    return iedmModel;
                }
            }

            if (errors != null && errors.GetEnumerator().Current != null)
            {
                var sb = new StringBuilder("Schema for Directory Resources is MalFormed. Errors:");
                foreach (EdmError error in errors)
                {
                    _ = sb.AppendLine(error.ErrorMessage);
                }

                throw new Exception(sb.ToString());
            }

            return model;
        }

        public static bool ParseModel(Stream stream, out IEdmModel model, out IEnumerable<EdmError> errors)
        {

            stream.Position = 0;
            var xmlReader = XmlReader.Create(stream);
            if (SchemaReader.TryParse(new[] { xmlReader }, out model, out errors))
            {
                return true;
            }

            stream.Position = 0;
            xmlReader = XmlReader.Create(stream);
            if (CsdlReader.TryParse(xmlReader, out model, out errors))
            {
                return true;
            }
            xmlReader.Dispose();

            return false;
        }
    }
}
