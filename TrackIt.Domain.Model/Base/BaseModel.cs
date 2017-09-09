using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Interfaces;

namespace TrackIt.Domain.Model.Base
{
    /// <summary>
    /// Abstract base class for all models.
    /// </summary>
    public abstract class BaseModel : IModel
    {
        /// <summary>
        /// The model's identity. Lower-case to better comply with JSON naming conventions.
        /// </summary>
        [Key]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
