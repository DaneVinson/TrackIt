using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Domain.Model;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Data.Dapper
{
    public class Repository<T> : CleanDisposable, IRepository<T> where T : class, IModel
    {
        public Repository(IDapperConfiguration configuration)
        {
            ConnectionString = configuration?.ConnectionString;
            Context = new EFContext(ConnectionString);
        }


        public async Task<bool> DeleteAsync(string id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var resultCount = await connection.ExecuteAsync($"delete {ModelUtility.GetPluralizedModelName<T>()} where id = @Id", new { Id = id });
                return resultCount == 1;
            }
        }

        public async Task<IEnumerable<T>> GetAsync(IQueryable<T> query)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var result = await connection.QueryAsync<T>(query.ToSql());
                return result.ToArray();
            }
        }

        public async Task<T> GetAsync(string id)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var result = await connection.QueryAsync<T>($"select * from {ModelUtility.GetPluralizedModelName<T>()} where id = @Id", new { Id = id });
                return result.FirstOrDefault();
            }
        }

        public IQueryable<T> GetQueryable()
        {
            return Context.Set<T>().AsNoTracking();
        }

        public async Task<T> UpsertAsync(T model)
        {
            string command = String.Empty;

            using (var connection = new SqlConnection(ConnectionString))
            {
                var ids = await connection.QueryAsync<string>("select top 1 id from DataPoints where id = @Id", model);
                if (ids == null || ids.Count() != 1)
                {
                    command = String.Format(
                                        "insert into {0} ({1}) values({2})",
                                        ModelUtility.GetPluralizedModelName<T>(),
                                        DapperUtility.GetPropertyNames<T>(),
                                        DapperUtility.GetPropertyNames<T>(true));
                }
                else
                {
                    command = String.Format(
                                        "update {0} set {1} where id = @Id",
                                        ModelUtility.GetPluralizedModelName<T>(),
                                        DapperUtility.GetPropertiesAssignmentSqlString<T>());
                }

                var upsertResult = await connection.ExecuteAsync(command, model);
                if (upsertResult == 1)
                {
                    connection.Close();
                    return model;
                }
                else
                {
                    connection.Close();
                    return null;
                }
            }
        }

        protected override void DisposeManagedResources()
        {
            // Nothing to dispose.
        }

        private readonly string ConnectionString;

        private readonly EFContext Context;

        private readonly string Id = Guid.NewGuid().ToString();
    }
}
