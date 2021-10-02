using D_API.SettingsTypes;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace D_API.Configurer.Lib
{
    public class APISettingsConfigBuilder : NotifyPropertyChanged
    {
        private string? encryptioniv;
        private string? encryptionkey;
        private long? eventChannelId;
        private string? jwtKey;
        private string? telegramKey;
        private string? hashKey;

        public virtual ObservableCollection<long> AllowedUsers { get; } = new();
        public virtual APISettingsSecurityBuilder Security { get; } = new();
        public virtual APISettingsDbConnectionBuilder Database { get; } = new();
        public virtual string? EncryptionIV { get => encryptioniv; set => NotifySet(ref encryptioniv, value); }
        public virtual string? EncryptionKey { get => encryptionkey; set => NotifySet(ref encryptionkey, value); }
        public virtual long? EventChannelId { get => eventChannelId; set => NotifySet(ref eventChannelId, value); }
        public virtual string? JWTKey { get => jwtKey; set => NotifySet(ref jwtKey, value); }
        public virtual string? TelegramAPIKey { get => telegramKey; set => NotifySet(ref telegramKey, value); }
        public virtual string? HashKey { get => hashKey; set => NotifySet(ref hashKey, value); }

        public virtual APISettings Build() => new()
        {
            AllowedUsers = new(AllowedUsers),
            EncryptionIV = encryptioniv,
            EncryptionKey = encryptionkey,
            EventChannelId = eventChannelId,
            JWTSecurityKey = jwtKey,
            Security = Security.Build(),
            TelegramAPIKey = telegramKey,
            UserSecretHashKey = hashKey,
            UserDataDbConnectionSettings = Database.Build()
        };

        public class APISettingsDbConnectionBuilder : NotifyPropertyChanged
        {
            private DbEndpoint endpoint;
            private string? connstring;

            public virtual DbEndpoint Endpoint { get => endpoint; set => NotifySet(ref endpoint, value); }
            public virtual string? ConnectionString { get => connstring; set => NotifySet(ref connstring, value); }

            public virtual DbConnectionSettings Build() => new(endpoint, connstring);
        }

        public class APISettingsSecurityBuilder : NotifyPropertyChanged
        {
            private string? audience;
            private string? issuer;

            public virtual string? Audience { get => audience; set => NotifySet(ref audience, value); }
            public virtual string? Issuer { get => audience; set => NotifySet(ref issuer, value); }

            public virtual Security Build() => new(audience, issuer);
        }
    }
}
