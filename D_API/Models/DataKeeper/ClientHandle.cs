using D_API.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Models.DataKeeper
{
    public class ClientHandle
    {
        [Key]
        public Guid Key { get; init; }
        public string Identifier { get; init; }

        public ClientHandle(Guid key, string identifier)
        {
            Key = key;
            Identifier = identifier;
        }
        public ClientHandle(Client client) : this(client.Key, client.Identifier) { }
    }
}
