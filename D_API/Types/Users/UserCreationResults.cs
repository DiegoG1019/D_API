using D_API.Dependencies.Interfaces;
using D_API.Enums;
using D_API.Types.Auth;
using DiegoG.Utilities.Measures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Types.Users
{
    public record UserCreationResults(UserCreationResult? Result, UserValidCredentials? Credentials, string? ReasonForDenial = null)
    {
        public IEnumerable<SerializedUserServiceData>? ServiceData { get; init; }
        public List<string> ServiceConfigurationResults { get; } = new();
    }

    public struct SerializedUserServiceData
    {
        public Service Service { get; init; }
        public object[] Data { get; init; }

        public SerializedUserServiceData(Service service, object[] data)
        {
            Service = service;
            Data = data;
        }

        public SUSDataConversionResults<IAppDataKeeper.DataKeeperQuotaSettings> ToDataKeeperQuotaSettings()
            => Data.Length >= 3 ? new(new(Data[0] as double?, Data[1] as double?, Data[2] as double?), true) : new(null, false);
    }

    public record SUSDataConversionResults<T>(T? Data, bool Success);

    public enum UserCreationResult
    {
        Accepted,
        AlreadyExists,
        Denied
    }
}
