using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;

namespace OData.Schema.Validation.Utils
{
    public class ParentValidator
    {
        Dictionary<string, IEdmModel> SourceSchemas;
        Dictionary<string, IEdmModel> DestinationSchemas;
        IEnumerable<EdmError> validationErrors;

        public ParentValidator (Dictionary<string, IEdmModel> sourceSchemas, Dictionary<string, IEdmModel> destinationSchemas)
        {
            SourceSchemas = sourceSchemas;
            DestinationSchemas = destinationSchemas;
        }

        public void RunValidateion()
        {
            validationErrors = ValdateSchema(DestinationSchemas);
        }

            public IEnumerable<EdmError> ValdateSchema(Dictionary<string, IEdmModel> schemas)
        {
            List<EdmError> validationErrorList = new List<EdmError>();

            foreach (var schema in schemas)
            {
                if (schema.Value != null)
                {
                    var ruleset = ValidationRuleSet.GetEdmModelRuleSet(new Version(4, 0));

                    IEnumerable<EdmError> validationErrors;
                    _ = schema.Value.Validate(ruleset, out validationErrors);
                    validationErrorList.AddRange(validationErrors);
                }
            }

            return validationErrorList;
        }

    }
}
