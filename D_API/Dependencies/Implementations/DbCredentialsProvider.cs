using D_API.DataContexts;
using D_API.Dependencies.Abstract;
using D_API.Models.Auth;
using D_API.Types.Auth;
using DiegoG.TelegramBot;
using DiegoG.Utilities.Settings;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using DiegoG.Utilities.IO;

namespace D_API.Dependencies.Implementations
{
    public class DbCredentialsProvider : AbstractAuthCredentialsProvider
    {
        private readonly ClientDataContext Db;

        public DbCredentialsProvider(string hashKey, ClientDataContext db) : base(hashKey) => Db = db;

        public override async Task<Client?> FindClient(Guid key) 
            => await Db.Clients.FindAsync(key);

        public override async Task<CredentialVerificationResults> Verify(ClientValidCredentials credentials)
        {
            Client client;
            bool fail = (client = await Db.Clients.FindAsync(credentials.Key)) is null;
            return await VerifyClientCredentials(credentials, client, fail);
        }
    }
}
