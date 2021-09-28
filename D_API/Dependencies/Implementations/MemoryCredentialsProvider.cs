using D_API.Dependencies.Abstract;
using D_API.Models.Auth;
using D_API.Types.Auth;
using DiegoG.Utilities.IO;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Dependencies.Implementations
{
    public class MemoryCredentialsProvider : AbstractAuthCredentialsProvider
    {
        private readonly Task<Dictionary<Guid, Client>> Clients;

        public MemoryCredentialsProvider(string hashKey, string? encryptionKey, string? encryptionIV) : base(hashKey)
        {
            const string file = "creds";
            string dir = Directories.InData("ClientCredentials");
            Directory.CreateDirectory(dir);

            if (!File.Exists(Path.Combine(dir, file + Serialization.JsonExtension)))
            {
                Log.Error("There's no file containing client data, creating empty memory cache");
                Clients = Task.FromResult(new Dictionary<Guid, Client>());
                return;
            }

            Clients = encryptionKey is not null && encryptionIV is not null
                ? DecryptAndDeserialize(dir, file, encryptionKey, encryptionIV)
                : GetDictionary(File.ReadAllText(Path.Combine(dir, file + Serialization.JsonExtension)));
        }

        private static async Task<Dictionary<Guid, Client>> DecryptAndDeserialize(string dir, string file, string enKey, string enIV)
            => await GetDictionary
                (await Helper.DecryptStringFromBytesAESAsync
                    (File.ReadAllBytes
                        (Path.Combine(dir, file + Serialization.JsonExtension)),
                     Encoding.UTF8.GetBytes(enKey),
                     Encoding.UTF8.GetBytes(enIV)));

        private static async Task<Dictionary<Guid, Client>> GetDictionary(string jsonstring)
        {
            var x = new Dictionary<Guid, Client>();
            var y = await Serialization.Deserialize<List<Client>>.JsonAsync(jsonstring);

            foreach (var z in y)
                x.Add(z.Key, z);
            return x;
        }

        public override async Task<Client?> FindClient(Guid key)
            => (await Clients).TryGetValue(key, out var client) ? client : null;

        public override async Task<CredentialVerificationResults> Verify(ClientValidCredentials credentials)
        {
            var l = await Clients;

            bool fail = l.TryGetValue(credentials.Key, out var client) is false;
            return await VerifyClientCredentials(credentials, client, fail);
        }
    }
}
