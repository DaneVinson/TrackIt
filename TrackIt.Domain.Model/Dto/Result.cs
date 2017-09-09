using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackIt.Domain.Model.Dto
{
    public class Result<T>
    {
        public string Message { get; set; }

        public bool Success { get; set; }

        public T Value { get; set; }
    }
}
