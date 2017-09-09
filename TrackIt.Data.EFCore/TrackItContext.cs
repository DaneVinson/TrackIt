using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackIt.Domain.Model.Interfaces;
using TrackIt.Domain.Model.Models;

namespace TrackIt.Data.EFCore
{
    public class TrackItContext : DbContext
    {
        public TrackItContext(IDbConfiguration dbConfiguration) : base()
        {
            ConnectionString = dbConfiguration.ConnectionString;
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
