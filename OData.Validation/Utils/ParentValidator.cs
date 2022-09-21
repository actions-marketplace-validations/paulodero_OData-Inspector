using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;

namespace OData.Schema.Validation.Utils
{
    public class SchemaValidator
    {
        private readonly Dictionary<string, IEdmModel> schema;
        public IEnumerable<EdmError> validationErrors;

        public SchemaValidator (Dictionary<string, IEdmModel> sourceSchemas)
        {
            schema = sourceSchemas;
            validationErrors = Enumerable.Empty<EdmError>();
        }

        public void RunValidation()
        {
            validationErrors = ValdateSchema(schema);
        }
        public static IEnumerable<EdmError> ValdateSchema(Dictionary<string, IEdmModel> schemas)
        {
            var validationErrorList = new List<EdmError>();
            foreach (var schema in schemas)
            {
                if (schema.Value != null)
                {
                    var ruleset = ValidationRuleSet.GetEdmModelRuleSet(new Version(4, 0));

                    _ = schema.Value.Validate(ruleset, out IEnumerable<EdmError> validationErrors);
                    validationErrorList.AddRange(validationErrors);
                }
            }

            return validationErrorList;
        }

    }
}
