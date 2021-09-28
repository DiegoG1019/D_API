using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace D_API.Models.Auth
{
    public class Client
    {
        public enum Status
        {
            /// <summary>
            /// This client was never active
            /// </summary>
            Inactive,

            /// <summary>
            /// This client is currently active and allowed access to the API
            /// </summary>
            Active,

            /// <summary>
            /// This client was once active, but is now revoked.
            /// </summary>
            Revoked
        }

        /// <summary>
        /// Comma separated string containing a client's active roles
        /// </summary>
        [Required]
        [StringLength(256, ErrorMessage = "Client.Roles Property can have a maximum length of 256 characters")]
        public string Roles { get; set; } = string.Empty;

        /// <summary>
        /// The Client's identifier
        /// </summary>
        [Required]
        [StringLength(128, ErrorMessage = "Client.Identifier Property can have a maximum length of 128 characters")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// The HASHED client secret
        /// </summary>
        [Required]
        [StringLength(128, ErrorMessage = "Secret")]
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// The Client's current status
        /// </summary>
        [Required]
        [ConcurrencyCheck]
        [EnumDataType(typeof(Status))]
        public Status CurrentStatus { get; set; } = Status.Inactive;

        [Key]
        public Guid Key { get; set; }

        public override string ToString() => $"{Identifier} ({Key})";
    }
}
