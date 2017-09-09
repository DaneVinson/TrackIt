using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Models;

namespace TrackIt.AdminConsole
{
    public class TrackItContext : DbContext
    {
        public TrackItContext() : base("TrackIt")
        { }

        public TrackItContext(string contextName) : base(contextName)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }


        public IDbSet<Category> Categories { get; set; }

        public IDbSet<DataPoint> DataPoints { get; set; }
    }
}
