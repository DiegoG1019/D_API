using System.ComponentModel.DataAnnotations;

namespace D_API.Types.Auth.Models;

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
    [StringLength(256, ErrorMessage = $"Client.{nameof(Roles)} Property can have a maximum length of 256 characters")]
    public string Roles { get; set; } = string.Empty;

    /// <summary>
    /// The Client's identifier
    /// </summary>
    [StringLength(128, ErrorMessage = $"Client.{nameof(Identifier)} Property can have a maximum length of 128 characters")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// The HASHED client secret
    /// </summary>
    [StringLength(128, ErrorMessage = $"{nameof(Secret)}")]
    public string Secret { get; set; } = string.Empty;
    
    /// <summary>
    /// The Client's current status
    /// </summary>
    [EnumDataType(typeof(Status))]
    public Status CurrentStatus { get; set; } = Status.Inactive;
    
    public Guid Key { get; set;}

    public override string ToString() => $"{Identifier} ({Key})";
}
