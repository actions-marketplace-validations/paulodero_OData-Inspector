using Microsoft.CodeAnalysis.Differencing;
using Microsoft.OData.Edm;

namespace OData.Schema.Validation.Models
{
    public class ModelContainer
    {
        public IEdmModel IedmModel { get; set; }
        public string Csdl { get; set; }

        public ModelContainer(IEdmModel iedmModel, string csdl)
        {
            IedmModel = iedmModel;
            Csdl = csdl;
        }

    }
}
