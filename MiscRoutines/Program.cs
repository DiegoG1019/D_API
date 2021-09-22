using D_API;
using D_API.Types.Auth;
using D_API.Types.Auth.Models;
using DiegoG.Utilities.IO;
using System.Text;

var clientid = await Serialization.Serialize.JsonAsync(new ClientValidCredentials(Guid.NewGuid(), await Helper.GenerateSecret(), "DiegoG"));

//File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\aasdw.txt", clientid, System.Text.Encoding.UTF8);

string secret = "jm1hqLt\u002B|l(le2R\u003E-^4ky7\u00A1RvR\u003EsV%wsE\u00F1\u006011;LG7Q)o):GfXE\u002BCjBCypD7\u0026SbxC";
string hashkey = "&fCd>2Cpxz=@SK>^sQkt5zV0aE]8IKNsqazFOPb:m-RBq0VsBSN?Ebn&^aiO6wE@";

var client = await Serialization.Serialize.JsonAsync(new Client()
{
    CurrentStatus = Client.Status.Active,
    Identifier = "DiegoG",
    Key = Guid.Parse("10158f3a-77d8-4ddc-847d-db209a070f0c"),
    Secret = await Helper.GetHash(secret, hashkey),
    Roles = "root"
});

File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\aasdw.txt", client, Encoding.UTF8);

;
//place a breakpoint here