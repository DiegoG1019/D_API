using D_API.Models.DataKeeper;
using D_API.Models.Auth;
using DiegoG.Utilities.Measures;
using Microsoft.EntityFrameworkCore;

namespace D_API.DataContexts;

public class ClientDataContext : DbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientDataTracker> ClientDataUsages { get; set; }
    public DbSet<DataEntry> DataEntries { get; set; }

#pragma warning disable CS8618
    public ClientDataContext(DbContextOptions<ClientDataContext> options) : base(options) { }
#pragma warning restore CS8618

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var adminClient = new Client()
        {
#warning Possible Security Risk, change this in a private repo, obtain it from some 
            CurrentStatus = Client.Status.Active,
            Identifier = "Administrator",
            Key = Guid.Parse("3a548bd0-492d-47b8-a768-548165318c77"),
            Roles = "root",
            Secret = "8391b391cb9fe95748780c96d3800b7517241f80f6c9749b59c57ccb586b104d24e7bb6ff7e7f522ab3cd1ffd2e16cfd580176077e0a353d68146a008b9987cd"
        }; 
        //If I put the unhashed secret here as well, it would absolutely be a very easy-to-miss security risk.
        //So, refer to MiscRoutines and gen a new one, or do your own thing here
        //It's still a security risk to have such a powerful account's id and hash out in the open but it can be removed later

        modelBuilder.Entity<Client>().HasData(adminClient);
        modelBuilder.Entity<ClientDataTracker>().HasData(new ClientDataTracker()
        {
            Key = Guid.Parse("3a548bd0-492d-47b8-a768-548165318c77"),
            DailyTransferQuota = new(10, Data.DataPrefix.Mega, 10, Data.DataPrefix.Mega),
            StorageQuota = new(10, Data.DataPrefix.Mega),
        });
        modelBuilder.Entity<DataEntry>().HasKey(d => new { d.Owner, d.Identifier });
    }
}
