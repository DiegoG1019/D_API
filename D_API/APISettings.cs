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

        public string? APIKey { get; set; }
        public List<long> AllowedUsers { get; set; } = new();
        public long EventChannelId { get; set; }

        public Security_settings Security { get; init; } = new(); public class Security_settings
        {
            public string? Audience { get; set; }
            public string? Issuer { get; set; }
        }
    }
}
