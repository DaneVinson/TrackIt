using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrackIt.Data.EFCore
{
    public interface IDbConfiguration
    {
        string ConnectionString { get; }
    }
}
