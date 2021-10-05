using D_API.DataContexts;
using D_API.Enums;
using D_API.Models.Auth;
using D_API.Models.DataKeeper;
using DiegoG.Utilities.Measures;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API
{
    public static class DbHelper
    {
        public static bool InitializeRootUser(UserDataContext db, string hashKey)
        {
            if (db.Database.EnsureCreated() || db.Users.Any(x => x.Identifier == "Root"))
                return false;

            string us = Helper.GenerateUnhashedSecret();
            var ui = Guid.NewGuid();

            Log.Warning($"Generated new Root User:\n-> User Secret: {us}\n-> Key: {ui}");

            var user = new User()
            {
                CurrentStatus = User.Status.Active,
                Identifier = "Root",
                Key = ui,
                Roles = UserAccessRoles.Root,
                Secret = Helper.GetHash(us, hashKey)
            };

            var handle = new UserHandle(user);

            var usertracker = new UserDataTracker()
            {
                Key = user.Key,
                DailyTransferDownloadQuota = Data.GetTotalBytes(50, Data.DataPrefix.Mega),
                DailyTransferUploadQuota = Data.GetTotalBytes(50, Data.DataPrefix.Mega),
                StorageQuota = Data.GetTotalBytes(20, Data.DataPrefix.Mega)
            };

            db.Users.Add(user);
            db.UserDataTrackers.Add(usertracker);
            db.UserHandles.Add(handle);

            db.SaveChanges();

            return true;
        }
    }
}
