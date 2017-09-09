using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackIt.Domain.Model.Interfaces
{
    /// <summary>
    /// Interface to which all model types should belong.
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// The model's identity.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        string Id { get; set; }
    }
}
