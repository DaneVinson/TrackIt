using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;

namespace TrackIt.Domain.Model.Models
{
    /// <summary>
    /// Class representing a value data point at a specific date/time.
    /// </summary>
    public sealed class DataPoint : BaseModel
    {
        /// <summary>
        /// The Id of the parent <see cref="Category"/>.
        /// </summary>
        public string CategoryId { get; set; }

        /// <summary>
        /// The date/time of the data point.
        /// </summary>
        public DateTime Stamp { get; set; }

        /// <summary>
        /// The value of the data point.
        /// </summary>
        public double Value { get; set; }


        public override string ToString()
        {
            return $"{Stamp} - {Value}";
        }
    }
}
