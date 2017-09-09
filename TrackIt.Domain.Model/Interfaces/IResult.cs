using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackIt.Domain.Model.Interfaces
{
    public interface IResult<T> where T : class
    {
        /// <summary>
        /// A message for the results.
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// The model object returned from the operation if any.
        /// </summary>
        T Model { get; set; }

        /// <summary>
        /// Was the operation successful?
        /// </summary>
        bool Success { get; set; }
    }
}
