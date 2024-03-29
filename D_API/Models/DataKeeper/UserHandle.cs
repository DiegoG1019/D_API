﻿using D_API.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Models.DataKeeper
{
    public class UserHandle
    {
        [Key]
        public Guid Key { get; init; }
        public string Identifier { get; init; }

        public UserHandle(Guid key, string identifier)
        {
            Key = key;
            Identifier = identifier;
        }
        public UserHandle(User user) : this(user.Key, user.Identifier) { }
    }
}
