using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Interfaces;

namespace TrackIt.Domain.Model.Dto
{
    public class Criteria<T> : ICriteria<T>
    {
        public string PrimaryId { get; set; }

        public T Value { get; set; }
    }
}
