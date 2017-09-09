using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackIt.Data.DocumentDB
{
    public interface IDocumentDBConfiguration
    {
        /// <summary>
        /// The Azure DocumentDB account key.
        /// </summary>
        string AccountKey { get; }

        /// <summary>
        /// The Azure DocumentDB account URI.
        /// </summary>
        string AccountUri { get; }

        /// <summary>
        /// The Azure DocumentDB name.
        /// </summary>
        string DatabaseName { get; }
    }
}
