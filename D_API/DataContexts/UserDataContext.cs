using D_API.Models.DataKeeper;
using D_API.Models.Auth;
using DiegoG.Utilities.Measures;
using Microsoft.EntityFrameworkCore;
using DiegoG.TelegramBot;
using DiegoG.Utilities.Settings;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using DiegoG.Utilities.IO;
using Microsoft.EntityFrameworkCore.Design;
using D_API.Models.DataKeeper.DataUsage.Complex;

namespace D_API.DataContexts
{
    public class UserDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserHandle> UserHandles { get; set; }
        public DbSet<UserDataTracker> UserDataUsages { get; set; }
        public DbSet<DataEntry> UserDataEntries { get; set; }

    #pragma warning disable CS8618
        public UserDataContext(DbContextOptions<UserDataContext> options) : base(options) { }
    #pragma warning restore CS8618

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DataEntry>().HasKey(d => new { d.Owner, d.Identifier });
        }
    }
}
