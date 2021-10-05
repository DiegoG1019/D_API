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

namespace D_API.DataContexts
{
    public class UserDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserHandle> UserHandles { get; set; }
        public DbSet<UserDataTracker> UserDataTrackers { get; set; }
        public DbSet<DataEntry> DataEntries { get; set; }

    #pragma warning disable CS8618
        public UserDataContext(DbContextOptions<UserDataContext> options) : base(options) { }
    #pragma warning restore CS8618

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable(nameof(Users));
            modelBuilder.Entity<UserHandle>().ToTable(nameof(UserHandles));
            modelBuilder.Entity<UserDataTracker>().ToTable(nameof(UserDataTrackers));
            modelBuilder.Entity<DataEntry>().ToTable(nameof(DataEntries));

            var user = new User()
            {
                CurrentStatus = User.Status.Inactive,
                Identifier = "Default",
                Key = Guid.NewGuid(),
                Roles = "",
                Secret = Helper.GetHash(Helper.GenerateUnhashedSecret(), Helper.GenerateRandomString(16))
            };

            modelBuilder.Entity<User>().HasData(user);

            modelBuilder.Entity<UserHandle>().HasData(new UserHandle(user));

            modelBuilder.Entity<UserDataTracker>().HasData(new UserDataTracker()
            {
                Key = user.Key,
                DailyTransferDownloadQuota = 0,
                DailyTransferUploadQuota = 0,
                StorageQuota = 0
            });

            modelBuilder.Entity<DataEntry>().HasKey(d => new { d.Owner, d.Identifier });
            modelBuilder.Entity<DataEntry>().HasData(new DataEntry(user.Key, "default", Array.Empty<byte>(), Guid.NewGuid(), new List<UserHandle>()));
        }
    }

}
