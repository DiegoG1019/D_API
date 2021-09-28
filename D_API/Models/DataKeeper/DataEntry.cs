using D_API.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace D_API.Models.DataKeeper
{
    public class DataEntry
    {
        public List<ClientHandle> Readers { get; init; }

        [Key, Column(Order = 0)]
        public Guid Owner { get; init; }

        [Key, Column(Order = 1)]
        public string Identifier { get; init; }
    
        public Guid ReadOnlyKey { get; init; }

        public byte[]? Data { get; set; }
    
        [NotMapped]
        public double Size => Data?.Length ?? 0;

        [NotMapped]
        public IEnumerable<Guid> ReadAccess => Readers.Select(x => x.Key).Prepend(Owner);

        /// <summary>
        /// If the Data is null or empty, it's not important
        /// </summary>
        [NotMapped]
        public bool IsImportant => Size > 0;

        private DataEntry(Guid owner, string identifier, byte[] data, Guid readOnlyKey) 
            : this(owner, identifier, data, readOnlyKey, new()) { }
        public DataEntry(Guid owner, string identifier, byte[] data, Guid readOnlyKey, List<ClientHandle> readers)
        {
            Readers = readers;
            Owner = owner;
            Identifier = identifier;
            ReadOnlyKey = readOnlyKey;
            Data = data;
        }
    }
}
