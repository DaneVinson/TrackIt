using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Data.Seed
{
    /// <summary>
    /// Get seed data specificically for Bilbo.
    /// </summary>
    public class BilbosSeedService
    {
        public static Category[] GetSeeds(SeedPack seedPack)
        {
            Category[] categories = new Category[0];
            switch (seedPack)
            {
                case SeedPack.Alpha:
                    categories = GetAlphaSeedPack();
                    break;
                case SeedPack.Bravo:
                    categories = GetBravoSeedPack();
                    break;
                case SeedPack.Charlie:
                    categories = GetCharlieSeedPack();
                    break;
                case SeedPack.Delta:
                case SeedPack.Echo:
                    break;
            }
            return categories;
        }

        private static Category[] GetAlphaSeedPack()
        {
            var categories = new List<Category>();
            string id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 36.2, 37.4, 39.6, 39.1, 42.3, 45.7, 48.2, 50.1}),
                Description = "Cornstalk Height",
                Id = id,
                Units = "Inches"
            });
            id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 2.2, 2.7, 3.0, 3.0, 3.1, 3.2, 3.5, 3.6 }),
                Description = "Beansprout Length",
                Id = id,
                Units = "Inches"
            });
            id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 1.2, 1.7, 0.5, 1.1, 3.1, 1.5, 1.6, 1.3 }),
                Description = "Sunflower Yield",
                Id = id,
                Units = "Pounds"
            });
            return categories.ToArray();
        }

        private static Category[] GetBravoSeedPack()
        {
            var categories = new List<Category>();
            string id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 6.8, 12.1, 3, 0, 10.2, 8, 8, 10 }),
                Description = "Travel",
                Id = id,
                Units = "Miles"
            });
            id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 4, 3, 0, 5, 2, 4, 6, 7 }),
                Description = "Gimli",
                Id = id,
                Units = "Orcs Slain"
            });
            id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 8, 12, 15, 8, 10, 13, 9, 21 }),
                Description = "Legolas",
                Id = id,
                Units = "Orcs Slain"
            });
            
            return categories.ToArray();
        }

        private static Category[] GetCharlieSeedPack()
        {
            var categories = new List<Category>();
            string id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 3, 1, 5, 2, 1, 8, 9, 14, 2, 1, 2, 4 }),
                Description = "Shooting Stars",
                Id = id,
                Units = "number"
            });
            id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 4, 8, 2, 0, 7, 1, 2, 6 }),
                Description = "Bird Species Spotted",
                Id = id,
                Units = "number"
            });
            id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 1, 0, 3, 2, 2, 2, 4, 0, 7 }),
                Description = "Letters Written",
                Id = id,
                Units = "number"
            });
            id = Guid.NewGuid().ToString();
            categories.Add(new Category()
            {
                AccountId = BilbosId,
                DataPoints = GenerateDataPoints(id, new double[] { 0.5, 0.1, 0, 0.1, 0.2, 0, 0, 0, 0.6, 0.1, 0, 1.1 }),
                Description = "Precipitation",
                Id = id,
                Units = "inches"
            });

            return categories.ToArray();
        }

        private static List<DataPoint> GenerateDataPoints(string categoryId, double[] values)
        {
            var dataPoints = new List<DataPoint>();
            int days = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                dataPoints.Add(new DataPoint()
                {
                    CategoryId = categoryId,
                    Id = Guid.NewGuid().ToString(),
                    Stamp = DateTime.Today.AddDays(-1 * days--),
                    Value = values[i]
                });
            }
            return dataPoints;
        }

        public static readonly string BilbosId = "537bd906-5a99-413d-823d-f7a4ae36ddfa";
    }
}
