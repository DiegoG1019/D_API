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
    public class ClientDataContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientHandle> ClientHandles { get; set; }
        public DbSet<ClientDataTracker> ClientDataUsages { get; set; }
        public DbSet<DataEntry> ClientDataEntries { get; set; }

    #pragma warning disable CS8618
        public ClientDataContext(DbContextOptions<ClientDataContext> options) : base(options) { }
    #pragma warning restore CS8618

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DataEntry>().HasKey(d => new { d.Owner, d.Identifier });
        }
    }
}
