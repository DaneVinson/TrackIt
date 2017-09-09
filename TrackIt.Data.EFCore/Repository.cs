using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Data.EFCore
{
    public class Repository<T> : CleanDisposable, IRepository<T> where T : class, IModel, new()
    {
        public Repository(IDbConfiguration configuration)
        {
            Configuration = configuration;
            Context = new TrackItContext(Configuration);
        }


        public async Task<bool> DeleteAsync(string id)
        {
            // This is a slick method to delete without a lookup but if the entity exists in the 
            // context's tracked entities it throws. Therefore, use a new context object.
            using (var context = new TrackItContext(Configuration))
            {
                var entity = new T();
                entity.Id = id;
                context.Attach(entity);
                context.Remove(entity);
                var saveCount = await context.SaveChangesAsync();
                return saveCount > 0;
            }
        }

        public async Task<IEnumerable<T>> GetAsync(IQueryable<T> query)
        {
            return await query.ToArrayAsync();
        }

        public async Task<T> GetAsync(string id)
        {
            return await Context.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public IQueryable<T> GetQueryable()
        {
            return Context.Set<T>().AsNoTracking();
        }

        public async Task<T> UpsertAsync(T model)
        {
            if (await Context.Set<T>().AnyAsync(e => e.Id == model.Id))
            {

                Context.Attach(model);
                Context.Entry(model).State = EntityState.Modified;
            }
            else { Context.Add(model); }
            var saveCount = await Context.SaveChangesAsync();
            if (saveCount > 0) { return model; }
            else { return null; }
        }


        protected override void DisposeManagedResources()
        {
            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }

        private readonly IDbConfiguration Configuration;

        protected TrackItContext Context { get; set; }
    }
}
