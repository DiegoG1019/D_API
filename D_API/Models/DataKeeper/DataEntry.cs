using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace D_API.Models.DataKeeper
{
    public class DataEntry
    {
        public List<Guid> Readers { get; init; } = new();

        [Key, Column(Order = 0)]
        public Guid Owner { get; init; } = new();

        [Key, Column(Order = 1)]
        public string Identifier { get; init; }
    
        public Guid ReadOnlyKey { get; init; }

        public byte[]? Data { get; set; }
    
        [NotMapped]
        public double Size => Data?.Length ?? 0;

        [NotMapped]
        public IEnumerable<Guid> ReadAccess => Readers.Prepend(Owner);

        /// <summary>
        /// If the Data is null or empty, it's not important
        /// </summary>
        [NotMapped]
        public bool IsImportant => Size > 0;

        public DataEntry(Guid owner, string identifier, byte[] dat, Guid readonlykey, params Guid[] readers)
        {
            Readers.AddRange(readers);
            Owner = owner;
            Identifier = identifier;
            ReadOnlyKey = readonlykey;
            Data = dat;
        }
    }
}
