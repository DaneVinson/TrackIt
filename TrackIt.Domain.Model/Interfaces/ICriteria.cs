using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackIt.Domain.Model.Interfaces
{
    /// <summary>
    /// Interface which defines a criteria object for operations against a persistence store.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICriteria<T>
    {
        /// <summary>
        /// The Id of the primary object.
        /// </summary>
        string PrimaryId { get; set; }

        /// <summary>
        /// The primary value object for the request.
        /// </summary>
        T Value { get; set; }
    }
}
