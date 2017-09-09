using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;

namespace TrackIt.Domain.Model.Models
{
    /// <summary>
    /// Class representing the defintion of a set of data points.
    /// </summary>
    public sealed class Category : BaseModel
    {
        public Category()
        {
            DataPoints = new List<DataPoint>();
        }


        /// <summary>
        /// The Id of the parent <see cref="Account"/>.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Collection of child DataPoint objects.
        /// </summary>
        public List<DataPoint> DataPoints { get; set; }

        /// <summary>
        /// A description of the category.
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string Description { get; set; }

        /// <summary>
        /// The units in which category's <see cref="DataPoint"/> values are recorded.
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string Units { get; set; }


        public override string ToString()
        {
            return $"{Description} ({Units})";
        }

    }
}
