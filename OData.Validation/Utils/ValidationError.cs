using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OData.Schema.Validation.Utils
{
    public class ValidationError
    {
        string ErrorMessage { get; set; }
        string ErrorType { get; set; }

        string ErrorLocation { get; set; }

    }
}
