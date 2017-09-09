using Dapper;
using Microsoft.Azure;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.Data.Dapper;
using TrackIt.Data.DocumentDB;
using TrackIt.Data.Seed;
using TrackIt.Domain.Model.Models;

namespace TrackIt.WebJob.BilboReset
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => ResetBilboData()).Wait();
        }


        private static async Task ResetBilboData()
        {
            var tasks = new List<Task>();
            if (ResetDocumentDB) { tasks.Add(ResetBilboDocumentDB(SeedPack.Charlie)); }
            if (ResetSqlAlpha) { tasks.Add(ResetBilboSql(SeedPack.Alpha)); }
            if (ResetSqlBravo) { tasks.Add(ResetBilboSql(SeedPack.Bravo)); }
            await Task.WhenAll(tasks);
        }

        private static async Task ResetBilboSql(SeedPack seedPack)
        {
            // SQL is Alpha or Bravo right now.
            string connectionString = seedPack == SeedPack.Alpha ? SqlAlphaConnectionString : SqlBravoConnectionString;

            // Delete current data. Dapper works great for this.
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Get the Ids of all Bilbo's Categories.
                var categoryIds = await connection.QueryAsync<string>(
                                                        "select Id from Categories where AccountId = @BilbosId", 
                                                        new { BilbosId = BilbosSeedService.BilbosId });

                // Delete the DataPoints of all Categories (cascade delete is not in use).
                var deleteCountTasks = new List<Task<int>>();
                foreach (var id in categoryIds)
                {
                    deleteCountTasks.Add(connection.ExecuteAsync(
                                                        "delete DataPoints where CategoryId = @CategoryId", 
                                                        new { CategoryId = id }));
                }
                var count = (await Task.WhenAll(deleteCountTasks)).Sum();
                Console.WriteLine($"{seedPack.ToString()} database: Delted {count} DataPoints");

                // Delete Bilbo's Categories.
                count = await connection.ExecuteAsync(
                                        "delete Categories where AccountId = @BilbosId",
                                        new { BilbosId = BilbosSeedService.BilbosId });
                Console.WriteLine($"{seedPack.ToString()} database: Delted {count} Categories");

                connection.Close();
            }

            // Get the new seed data.
            var seedData = BilbosSeedService.GetSeeds(seedPack);

            // Reload seed data with TrackIt.Data.Dapper repositories.
            using (var catetoriesRepository = new Data.Dapper.Repository<Category>(new DapperConfiguration(connectionString)))
            {
                var categoryTasks = new List<Task<Category>>();
                foreach(var category in seedData)
                {
                    categoryTasks.Add(catetoriesRepository.UpsertAsync(category));
                }
                var createdCategories = await Task.WhenAll(categoryTasks);
                Console.WriteLine($"{seedPack.ToString()} database: Created {createdCategories?.Count()} Categories");
            }
            using (var dataPointsRepository = new Data.Dapper.Repository<DataPoint>(new DapperConfiguration(connectionString)))
            {
                var dataPoints = new List<DataPoint>();
                foreach (var category in seedData)
                {
                    dataPoints.AddRange(category.DataPoints ?? new List<DataPoint>());
                }

                var dataPointTasks = new List<Task<DataPoint>>();
                dataPointTasks.AddRange(seedData.SelectMany(c => c.DataPoints)
                                                .Select(d => dataPointsRepository.UpsertAsync(d)));
                var createdDataPoints = await Task.WhenAll(dataPointTasks);
                Console.WriteLine($"{seedPack.ToString()} database: Created {createdDataPoints?.Count()} DataPoints");
            }
        }

        private static async Task ResetBilboDocumentDB(SeedPack seedPack)
        {
            using (var categoriesRepository = new TrackIt.Data.DocumentDB.Repository<Category>(AzureDocumentDBConfiguration))
            using (var dataPointsRepository = new TrackIt.Data.DocumentDB.Repository<DataPoint>(AzureDocumentDBConfiguration))
            {
                // Delete all current DataPoints then Categories
                var deleteTasks = new List<Task<bool>>();

                var categories = categoriesRepository.GetQueryable().ToList();
                foreach (var category in categories)
                {
                    deleteTasks.Add(categoriesRepository.DeleteAsync(category.Id));
                }

                var dataPoints = dataPointsRepository.GetQueryable().ToList();
                foreach (var dataPoint in dataPoints)
                {
                    deleteTasks.Add(dataPointsRepository.DeleteAsync(dataPoint.Id));
                }

                var results = await Task.WhenAll(deleteTasks);
                Console.WriteLine($"DocumentDB: Deleted {results.Select(r => r == true).Count()} of {results.Count()} documents.");

                // Add new DataPoints
                var seeds = BilbosSeedService.GetSeeds(seedPack);
                categories = new List<Category>();
                var dataPointsTasks = new List<Task<DataPoint>>();
                foreach (var seed in seeds)
                {
                    seed.DataPoints.ForEach(d => dataPointsTasks.Add(dataPointsRepository.UpsertAsync(d)));
                    seed.DataPoints = null;
                    categories.Add(seed);
                }
                dataPoints = (await Task.WhenAll(dataPointsTasks))?.ToList();
                Console.WriteLine($"DocumentDB: Created {dataPoints?.Count} DataPoints");

                // Add new Categoires
                var categoriesTasks = new List<Task<Category>>();
                categories.ForEach(c =>
                {
                    categoriesTasks.Add(categoriesRepository.UpsertAsync(c));
                });
                categories = (await Task.WhenAll(categoriesTasks))?.ToList();
                Console.WriteLine($"DocumentDB: Created {categories?.Count} Categories");
            }
        }

        #region Plumbing

        static Program()
        {
            AzureDocumentDBConfiguration = new DocumentDBConfiguration(
                                                   CloudConfigurationManager.GetSetting("AzureDocumentDBAccountKey"),
                                                   CloudConfigurationManager.GetSetting("AzureDocumentDBAccountUri"),
                                                   CloudConfigurationManager.GetSetting("AzureDocumentDBDatabaseName"));
            ResetDocumentDB = Boolean.TryParse(CloudConfigurationManager.GetSetting("ResetDocumentDB"), out ResetDocumentDB) && ResetDocumentDB;
            ResetSqlAlpha = Boolean.TryParse(CloudConfigurationManager.GetSetting("ResetSqlAlpha"), out ResetSqlAlpha) && ResetSqlAlpha;
            ResetSqlBravo = Boolean.TryParse(CloudConfigurationManager.GetSetting("ResetSqlBravo"), out ResetSqlBravo) && ResetSqlBravo;
            SqlAlphaConnectionString = ConfigurationManager.ConnectionStrings["SqlAlphaConnectionString"].ConnectionString;
            SqlBravoConnectionString = ConfigurationManager.ConnectionStrings["SqlBravoConnectionString"].ConnectionString;
        }

        private static readonly IDocumentDBConfiguration AzureDocumentDBConfiguration;
        private static readonly bool ResetSqlAlpha;
        private static readonly bool ResetSqlBravo;
        private static readonly bool ResetDocumentDB;
        private static readonly string SqlAlphaConnectionString;
        private static readonly string SqlBravoConnectionString;

        #endregion
    }
}
