using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace D_API.Lib.Types
{
    public class Credentials
    {
        public string Key { get; private set; }
        public string Secret { get; private set; }
        public string Identifier { get; private set; }

        public Credentials(string key, string secret, string identifier)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            if(secret is null)
                throw new ArgumentNullException(nameof(secret));
            if(identifier is null) 
                throw new ArgumentNullException(nameof(identifier));

            Key = key;
            Secret = secret;
            Identifier = identifier;
        }

        internal string ToJson()
            => JsonSerializer.Serialize(this);
    }
}
