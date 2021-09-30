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
using D_API.Models.DataKeeper;
using DiegoG.Utilities.Measures;
using D_API.Types.Users;

namespace D_API.Dependencies.Implementations
{
    public class DbCredentialsProvider : AbstractAuthCredentialsProvider
    {
        private readonly UserDataContext Db;

        public DbCredentialsProvider(string hashKey, UserDataContext db) : base(hashKey) => Db = db;

        public override async Task<User?> FindUser(Guid key) 
            => await Db.Users.FindAsync(key);

        public override async Task<CredentialVerificationResults> Verify(UserValidCredentials credentials)
        {
            User user;
            bool fail = (user = await Db.Users.FindAsync(credentials.Key)) is null;
            return await VerifyUserCredentials(credentials, user, fail);
        }

        public override bool EnsureRoot() => DbHelper.InitializeRootUser(Db, HashKey);

        public override async Task<UserCreationResults> Create(UserCreationData newUser)
        {
            var r = await VerifyUserCreation(newUser, await Task.Run(() => Db.Users.FirstOrDefault(x => x.Identifier == newUser.Identifier)), false);
            if(r.Result is UserCreationResult.Accepted)
            {
                if (r.Credentials is null)
                    throw new InvalidOperationException("VerifyUserCreation resulted in an Accepted response, but the returned Credentials were null");
                var newu = new User()
                {
                    CurrentStatus = User.Status.Active,
                    Identifier = newUser.Identifier,
                    Roles = newUser.Roles,
                    Key = r.Credentials.Key,
                    Secret = await Helper.GetHashAsync(r.Credentials.Secret, HashKey)
                };
                Db.Users.Add(newu);
                Db.UserHandles.Add(new(newu));
                Db.UserDataTrackers.Add(UserDataTracker.Empty(newu.Key));
                await Db.SaveChangesAsync();
            }
            return r;
        }
    }
}
