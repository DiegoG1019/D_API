using D_API;
using D_API.Models.Auth;
using D_API.Types.Auth;
using DiegoG.Utilities.IO;
using System.Text;

string hashkey = "&fCd>2Cpxz=@SK>^sQkt5zV0aE]8IKNsqazFOPb:m-RBq0VsBSN?Ebn&^aiO6wE@";

// Generate a new secret and its hashed countepart
var secret = Helper.GenerateUnhashedSecret();
var hashedsecret = Helper.GetHash(secret, hashkey);

// Generate a new client file
/*
File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\aasdw.txt", clientid, System.Text.Encoding.UTF8);

var client = await Serialization.Serialize.JsonAsync(new Client()
{
    CurrentStatus = Client.Status.Active,
    Identifier = "DiegoG",
    Key = Guid.Parse("10158f3a-77d8-4ddc-847d-db209a070f0c"),
    Secret = await Helper.GetHashAsync(secret, hashkey),
    Roles = "root"
});

File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\aasdw.txt", client, Encoding.UTF8);*/

;//place a breakpoint here