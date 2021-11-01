

using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Entities
{


    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<SavingsRequest> SavingsRequests { get; set; }
        public DbSet<SavingsPlan> SavingsPlans { get; set; }

        public DbSet<TopWithdrwalActivity> TopUpWithdrwalActivity { get; set; }

        public DbSet<AppClient> AppClient { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

        }


    }


}