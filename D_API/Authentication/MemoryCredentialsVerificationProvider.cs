using D_API.Interfaces;
using D_API.Types.Auth;
using D_API.Types.Auth.Models;
using System.IO;
using System.Text;

namespace D_API.Authentication
{
    public class MemoryCredentialsVerificationProvider : IAuthCredentialsVerificationProvider
    {
        private readonly Task<Dictionary<Guid, Client>> Clients;
        private readonly string HashKey;

        public MemoryCredentialsVerificationProvider(string hashkey, string? encryptionKey, string? encryptionIV)
        {
            const string file = "creds";
            string dir = Directories.InData("ClientCredentials");
            Directory.CreateDirectory(dir);

            HashKey = hashkey;

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
                (await Helper.DecryptStringFromBytesAES
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

        public async Task<Client?> FindClient(Guid key) 
            => !(await Clients).TryGetValue(key, out var client) ? client : null;

        public async Task<CredentialVerificationResults> Verify(ClientValidCredentials credentials)
        {
            var l = await Clients;
            return l.TryGetValue(credentials.Key, out var client)
                ? client.CurrentStatus is Client.Status.Active
                    ? client.Identifier == credentials.Identifier && client.Secret == await Helper.GetHash(credentials.Secret, HashKey)
                        ? new CredentialVerificationResults(CredentialVerificationResult.Authorized, client)
                        : new CredentialVerificationResults(CredentialVerificationResult.Unauthorized, client)
                    : client.CurrentStatus is Client.Status.Revoked
                    ? new CredentialVerificationResults(CredentialVerificationResult.Revoked, client)
                    : new CredentialVerificationResults(CredentialVerificationResult.Forbidden, client)
                : new CredentialVerificationResults(CredentialVerificationResult.Forbidden, client);
        }
    }
}
