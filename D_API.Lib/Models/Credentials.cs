using System;
using System.Collections.Generic;
using System.Text;

namespace D_API.Lib.Models
{
    public sealed class Credentials
    {
        public string UserKey { get; set; }
        public string Identifier { get; set; }
        public string Secret { get; set; }

        public Credentials(string userKey, string identifier, string secret)
        {
            UserKey = userKey;
            Identifier = identifier;
            Secret = secret;
        }

        public Credentials(Guid userKey, string identifier, string secret) : this(userKey.ToString(), identifier, secret) { }
    }
}
