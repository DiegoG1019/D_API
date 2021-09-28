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
        private readonly Task<Dictionary<Guid, User>> Users;

        public MemoryCredentialsProvider(string hashKey, string? encryptionKey, string? encryptionIV) : base(hashKey)
        {
            const string file = "creds";
            string dir = Directories.InData("UserCredentials");
            Directory.CreateDirectory(dir);

            if (!File.Exists(Path.Combine(dir, file + Serialization.JsonExtension)))
            {
                Log.Error("There's no file containing user data, creating empty memory cache");
                Users = Task.FromResult(new Dictionary<Guid, User>());
                return;
            }

            Users = encryptionKey is not null && encryptionIV is not null
                ? DecryptAndDeserialize(dir, file, encryptionKey, encryptionIV)
                : GetDictionary(File.ReadAllText(Path.Combine(dir, file + Serialization.JsonExtension)));
        }

        private static async Task<Dictionary<Guid, User>> DecryptAndDeserialize(string dir, string file, string enKey, string enIV)
            => await GetDictionary
                (await Helper.DecryptStringFromBytesAESAsync
                    (File.ReadAllBytes
                        (Path.Combine(dir, file + Serialization.JsonExtension)),
                     Encoding.UTF8.GetBytes(enKey),
                     Encoding.UTF8.GetBytes(enIV)));

        private static async Task<Dictionary<Guid, User>> GetDictionary(string jsonstring)
        {
            var x = new Dictionary<Guid, User>();
            var y = await Serialization.Deserialize<List<User>>.JsonAsync(jsonstring);

            foreach (var z in y)
                x.Add(z.Key, z);
            return x;
        }

        public override async Task<User?> FindUser(Guid key)
            => (await Users).TryGetValue(key, out var user) ? user : null;

        public override async Task<CredentialVerificationResults> Verify(UserValidCredentials credentials)
        {
            var l = await Users;

            bool fail = l.TryGetValue(credentials.Key, out var user) is false;
            return await VerifyUserCredentials(credentials, user, fail);
        }
    }
}
