using D_API;
using D_API.Models.Auth;
using D_API.Types.Auth;
using DiegoG.Utilities.IO;
using System.Text;
using D_API.Models.DataKeeper;
using System;
using System.IO;
using MessagePack;

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
