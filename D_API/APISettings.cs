using D_API.SettingsTypes;
using DiegoG.Utilities.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace D_API
{
    public class APISettings : ISettings
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string SettingsType => "APISettings";
        public ulong Version => 2;

        public string? TelegramAPIKey { get; init; }
        public List<long> AllowedUsers { get; init; } = new() { 0 };
        public long? EventChannelId { get; init; } = 0;
        
        public string? ClientSecretHashKey { get; set; }

        public DbConnectionSettings ClientDataDbConnectionSettings { get; init; } = new();

        public string? JWTSecurityKey { get; init; }

        public string? EncryptionKey { get; init; }
        public string? EncryptionIV { get; init; }

        public Security Security { get; init; } = new(); 
    }
}

namespace D_API.SettingsTypes
{
    public enum DbEndpoint
    {
        NoDB,
        SQLServer,
        CosmosDB
    }

    public sealed record Security(string? Audience, string? Issuer)
    {
        public Security() : this (null, null) { }
    }

    public sealed record DbConnectionSettings(DbEndpoint Endpoint, string? ConnectionString)
    {
        public DbConnectionSettings() : this(DbEndpoint.SQLServer, null) { }
    }
}