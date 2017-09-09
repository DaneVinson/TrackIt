using log4net;
using log4net.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackIt.AdminConsole.Migrations;
using TrackIt.Data.Dapper;
using TrackIt.Data.Seed;
using TrackIt.Domain.Logic.Managers;
using TrackIt.Domain.Model.Base;
using TrackIt.Domain.Model.Dto;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.AdminConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // log4net
            XmlConfigurator.Configure();

            try
            {
                //EFMigration("TrackItAlpha");
                SeedSqlData("TrackItAlpha", SeedPack.Alpha);
            }
            catch (Exception ex)
            { Log.Error("Exception", ex); }
            finally
            {
                Console.WriteLine($"{Environment.NewLine}---");
                Console.ReadKey();
            }
        }


        private static void DapperTest()
        {
            using (var repository = new TrackIt.Data.Dapper.Repository<DataPoint>(new DapperConfiguration("")))
            {
                var q = repository.GetQueryable();
                q = q.Where(d => d.Value > 40);
                var task = repository.GetAsync(q);

                // UpsertAsync
                //var dataPoint = new DataPoint()
                //{
                //    CategoryId = "0f0d1cd7-7d1f-400e-a5a1-0cd6e87d71d6",
                //    Id = Guid.NewGuid().ToString(),
                //    Stamp = DateTime.UtcNow,
                //    Value = 25
                //};
                //var task = repository.UpsertAsync(dataPoint);

                task.Wait();
                var result = task.Result;
            }
        }

        public static void EFMigration(string connectionName)
        {
            var dbConnectionInfo = new DbConnectionInfo(connectionName);

            DbMigrator migrator = new DbMigrator(new Configuration() { TargetDatabase = dbConnectionInfo });

            foreach (var migration in migrator.GetDatabaseMigrations())
            {
                Console.WriteLine($"Previous Migration: {migration}");
            }

            foreach (var migration in migrator.GetPendingMigrations())
            {
                Console.WriteLine($"Pending migration: {migration}");
                migrator.Update(migration);
                Console.WriteLine("Migration {0} complete.", migration);
                Console.WriteLine();
            }
        }

        private static void SeedSqlData(string connectionName, SeedPack seedPack)
        {
            using (var context = new TrackItContext(connectionName))
            {
                if (context.Categories.Any() || context.DataPoints.Any())
                {
                    Console.WriteLine("Database can't be seeded because it isn't empty.");
                    return;
                }
                BilbosSeedService.GetSeeds(seedPack)
                                    .ToList()
                                    .ForEach(c => context.Categories.Add(c));
                var addCount = context.SaveChanges();
                Console.WriteLine($"Seed pack {seedPack} planted on {connectionName} data store ({addCount} records)");
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
    }
}
