using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace D_API.Models.Auth
{
    public class User
    {
        public enum Status
        {
            /// <summary>
            /// This user was never active
            /// </summary>
            Inactive,

            /// <summary>
            /// This user is currently active and allowed access to the API
            /// </summary>
            Active,

            /// <summary>
            /// This user was once active, but is now revoked.
            /// </summary>
            Revoked
        }

        /// <summary>
        /// Comma separated string containing a user's active roles
        /// </summary>
        [Required]
        [StringLength(256, ErrorMessage = "User.Roles Property can have a maximum length of 256 characters")]
        public string Roles { get; set; } = string.Empty;

        /// <summary>
        /// The User's identifier
        /// </summary>
        [Required]
        [StringLength(128, ErrorMessage = "User.Identifier Property can have a maximum length of 128 characters")]
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// The HASHED user secret
        /// </summary>
        [Required]
        [StringLength(128, ErrorMessage = "Secret")]
        public string Secret { get; set; } = string.Empty;

        /// <summary>
        /// The User's current status
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
