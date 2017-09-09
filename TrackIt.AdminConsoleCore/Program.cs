using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TrackIt.Data.Dapper;
using TrackIt.Domain.Model.Models;

namespace TrackIt.AdminConsoleCore
{
    class Program
    {
        static Program()
        {
            var configurationBuilder = new ConfigurationBuilder()
                                                .SetBasePath(Directory.GetCurrentDirectory())
                                                .AddJsonFile("config.json")
                                                .AddEnvironmentVariables();
            Configuration = configurationBuilder.Build();
        }


        static void Main(string[] args)
        {
            try
            {
                var configuration = new DapperConfiguration(Configuration.GetConnectionString("TrackItAlpha"));
                using (var repository = new Repository<Category>(configuration))
                {
                    var q = repository.GetQueryable().Where(c => c.Id == "34c87b4d-a0a2-46de-acf2-2c1826b5fc8b");
                    var r = repository.GetAsync(q).Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} - {1}", ex.GetType(), ex.Message);
                Console.WriteLine(ex.StackTrace ?? String.Empty);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("...");
                Console.ReadKey();
            }
        }

        private static IConfigurationRoot Configuration { get; set; }
    }
}