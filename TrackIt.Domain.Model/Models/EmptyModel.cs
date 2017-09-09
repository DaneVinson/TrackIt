using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Interfaces;

namespace TrackIt.Domain.Model.Base
{
    /// <summary>
    /// A completely empty IModel to supply as a type when using generics but no type is actually needed.
    /// </summary>
    public sealed class EmptyModel : IModel
    {
        public string Id { get; set; }
    }
}
