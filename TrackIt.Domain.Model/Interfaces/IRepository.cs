using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Domain.Model.Interfaces
{
    public interface IRepository<T> : IDisposable where T : class, IModel
    {
        Task<bool> DeleteAsync(string id);

        Task<T> GetAsync(string id);

        Task<IEnumerable<T>> GetAsync(IQueryable<T> query);

        IQueryable<T> GetQueryable();

        Task<T> UpsertAsync(T model);
    }
}
