using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackIt.Data.Dapper
{
    public interface IDapperConfiguration
    {
        /// <summary>
        /// The primary connection string.
        /// </summary>
        string ConnectionString { get; }
    }
}
