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
        public ulong Version => 1;

        public string? TelegramAPIKey { get; set; }
        public List<long> AllowedUsers { get; set; } = new() { 0 };
        public long? EventChannelId { get; set; } = 0;
        
        public string? ClientSecretHashKey { get; set; }

        public string? JWTSecurityKey { get; set; }

        public string? EncryptionKey { get; set; }
        public string? EncryptionIV { get; set; }

        public Security_settings Security { get; init; } = new(); public class Security_settings
        {
            public string? Audience { get; set; }
            public string? Issuer { get; set; }
        }
    }
}
