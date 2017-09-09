using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Dto;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Domain.Model.Interfaces
{
    public interface IDataPointManager
    {
        Task<Result<EmptyModel>> DeleteAsync(Criteria<IEnumerable<string>> idsCriteria);

        Task<Result<DataPoint[]>> UpsertAsync(Criteria<IEnumerable<DataPoint>> dataPointsCriteria);
    }
}
