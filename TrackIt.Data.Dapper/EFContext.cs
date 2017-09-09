using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Data.Dapper
{
    internal class EFContext : DbContext
    {
        public EFContext(string connectionString): base()
        {
            ConnectionString = connectionString;
        }


        internal IQueryable<T> GetQuerable<T>()
        {
            if (typeof(T) == typeof(Category)) { return Categories.AsQueryable() as IQueryable<T>; }
            if (typeof(T) == typeof(DataPoint)) { return DataPoints.AsQueryable() as IQueryable<T>; }
            return null;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }


        public DbSet<Category> Categories { get; set; }

        public DbSet<DataPoint> DataPoints { get; set; }


        private readonly string ConnectionString;
    }
}
