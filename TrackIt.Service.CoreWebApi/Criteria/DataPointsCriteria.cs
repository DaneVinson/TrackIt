using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrackIt.Service.CoreWebApi.Criteria
{
    public class DataPointsCriteria
    {
        public string CategoryId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
