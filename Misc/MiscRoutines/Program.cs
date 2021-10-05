using D_API;
using D_API.Models.Auth;
using D_API.Types.Auth;
using DiegoG.Utilities.IO;
using System.Text;
using D_API.Models.DataKeeper;
using System;
using System.IO;
using MessagePack;
using D_API.Enums;
using System.Collections.Generic;

string hashkey = "&fCd>2Cpxz=@SK>^sQkt5zV0aE]8IKNsqazFOPb:m-RBq0VsBSN?Ebn&^aiO6wE@";

#region Generate a new secret and its hashed countepart
//var secret = Helper.GenerateUnhashedSecret();
//var hashedsecret = Helper.GetHash(secret, hashkey);
#endregion

#region Generate a new client file

//var user = await Serialization.Serialize.JsonAsync(new User()
//{
//    CurrentStatus = User.Status.Active,
//    Identifier = "DiegoG",
//    Key = Guid.Parse("10158f3a-77d8-4ddc-847d-db209a070f0c"),
//    Secret = await Helper.GetHashAsync(secret, hashkey),
//    Roles = "root"
//});

#endregion

#region Test DailyUsageTracker
//var x = new DailyUsageTracker(15, 2);
//var serialx = x.Serialize();
//var deserialx = DailyUsageTracker.Deserialize(serialx);
#endregion

#region Test UserDataTracker

//var tracker = new UserDataTracker()
//{
//    DailyTransferDownloadQuota = 13251,
//    DailyTransferUploadQuota = 21231,
//    StorageQuota = 24,
//    StorageUsage = 1231
//};

//await tracker.LoadTrackers();

//await tracker.AddTracker(new(12, 3 ));
//await tracker.AddTracker(new(1 , 53));
//await tracker.AddTracker(new(2 , 15));
//await tracker.AddTracker(new(7 , 9 ));
//await tracker.AddTracker(new(4 , 21));

//await tracker.SaveTrackers();

#endregion