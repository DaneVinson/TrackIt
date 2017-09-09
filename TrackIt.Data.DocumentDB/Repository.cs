using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Extensions;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Data.DocumentDB
{
    public class Repository<T> : CleanDisposable, IRepository<T> where T : class, IModel
    {
        public Repository(IDocumentDBConfiguration configuration)
        {
            Configuration = configuration;
            Context = DocumentDBUtility.NewDocumentClient(configuration);
        }


        public async Task<bool> DeleteAsync(string id)
        {
            // DeleteDocumentAsync thows if the document link is invalid.
            // Get returns null so verify existence before calling delete.
            var exists = (await GetAsync(id)) != null;
            if (!exists) { return false; }

            // Delete the requested account.
            var response = await Context.DeleteDocumentAsync(
                                            DocumentDBUtility.GetDocumentLink<T>(id, Configuration),
                                            new RequestOptions() { PartitionKey = new PartitionKey(id) });
            return response.StatusCode.IsSuccessResponse();
        }

        public async Task<T> GetAsync(string id)
        {
            var result = await GetAsync(GetQueryable().Where(a => a.Id == id));
            return result?.FirstOrDefault();
        }

        public async Task<IEnumerable<T>> GetAsync(IQueryable<T> query)
        {
            var documentQuery = query.AsDocumentQuery();
            List<T> list = new List<T>();
            while (documentQuery.HasMoreResults)
            {
                var result = await query.AsDocumentQuery().ExecuteNextAsync<T>();
                if (result != null && result.Count > 0) { list.AddRange(result.ToArray()); }
            }
            return list;
        }

        public IQueryable<T> GetQueryable()
        {
            var queryOptions = DocumentDBUtility.GetDocumentFeed<T>(Configuration);
            return Context.CreateDocumentQuery<T>(queryOptions, new FeedOptions { EnableCrossPartitionQuery = true });
        }

        public async Task<T> UpsertAsync(T model)
        {
            var response = await Context.UpsertDocumentAsync(DocumentDBUtility.GetDocumentFeed<T>(Configuration), model);
            if (response.StatusCode.IsSuccessResponse()) { return model; }
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

        protected readonly IDocumentDBConfiguration Configuration;

        protected DocumentClient Context { get; set; }
    }
}
