using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;

namespace TrackIt.Domain.Model.Extensions
{
    /// <summary>
    /// Extension methods for IModel and Model types.
    /// </summary>
    public static class ModelExtender
    {
        /// <summary>
        /// Method to perform validation on a model's data annotated properties.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static bool TryValidateModel(this BaseModel model, out string[] results)
        {
            if (model == null)
            {
                results = new string[] { "The model is null." };
                return false;
            }
            var validationContext = new ValidationContext(model, null, null);
            var resultsList = new List<ValidationResult>();
            var valid = Validator.TryValidateObject(model, validationContext, resultsList, true);
            results = resultsList.Select(r => r.ErrorMessage).ToArray();
            return valid;
        }
    }
}
