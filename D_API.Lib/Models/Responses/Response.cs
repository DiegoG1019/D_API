using D_API.Lib.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace D_API.Lib.Models.Responses
{
    public enum APIResponseCode : ushort
    {
        UnspecifiedError = 0, TooManyRequests = 1, BadUserKey = 2,

        Message = 100,

        NewSessionSuccess = 200, NewSessionFailure = 201, NewSessionBadRequest = 202,
        RenewSessionSuccess = 210, RenewSessionFailure = 211,
        RoleReport = 220,
        AuthStatus = 230,

        DataUploadSuccess = 300, DataUploadFailure = 301,
        DataDownloadSuccess = 310, DataDownloadFailure = 311,
        TransferQuotaStatus = 320, DataQuotaExceeded = 321,
        BadDataKey = 330,
        AccessCheck = 340,

        NewUserSuccess = 400, NewUserFailure = 401
    }

    public abstract class APIResponse
    {
        public APIResponseCode APIResponseCode { get; set; }
        public string Title { get; set; }

        public string Serialize() => JsonSerializer.Serialize(this);
        public override string ToString() => Serialize();

        private static T Deserialize<T>(string jsonString) where T : APIResponse => JsonSerializer.Deserialize<T>(jsonString)!;
        public static APIResponseCode GetResponseCode(string jsonString) => (APIResponseCode)JsonDocument.Parse(jsonString).RootElement.GetProperty("APIResponseCode").GetUInt16();
        public static T GetResponse<T>(string jsonString) where T : APIResponse => Deserialize<T>(jsonString)!;
        public static APIResponse GetResponse(string jsonString)
            => GetResponseCode(jsonString) switch
            {
                APIResponseCode.BadDataKey => Deserialize<BadDataKeyResponse>(jsonString),
                APIResponseCode.UnspecifiedError => Deserialize<UnspecifiedErrorResponse>(jsonString),
                APIResponseCode.TooManyRequests => Deserialize<TooManyRequestsResponse>(jsonString),
                APIResponseCode.BadUserKey => Deserialize<BadUserKeyResponse>(jsonString),
                APIResponseCode.Message => Deserialize<MessageResponse>(jsonString),
                APIResponseCode.NewSessionSuccess => Deserialize<NewSessionSuccessResponse>(jsonString),
                APIResponseCode.NewSessionFailure => Deserialize<NewSessionFailureResponse>(jsonString),
                APIResponseCode.NewSessionBadRequest => Deserialize<NewSessionBadRequestResponse>(jsonString),
                APIResponseCode.RenewSessionSuccess => Deserialize<RenewSessionSuccessResponse>(jsonString),
                APIResponseCode.RenewSessionFailure => Deserialize<RenewSessionFailureResponse>(jsonString),
                APIResponseCode.RoleReport => Deserialize<RoleReportResponse>(jsonString),
                APIResponseCode.AuthStatus => Deserialize<AuthStatusResponse>(jsonString),
                APIResponseCode.DataUploadSuccess => Deserialize<DataUploadSuccessResponse>(jsonString),
                APIResponseCode.DataUploadFailure => Deserialize<DataUploadFailureResponse>(jsonString),
                APIResponseCode.DataDownloadSuccess => Deserialize<DataDownloadSuccessResponse>(jsonString),
                APIResponseCode.DataDownloadFailure => Deserialize<DataDownloadFailureResponse>(jsonString),
                APIResponseCode.TransferQuotaStatus => Deserialize<TransferQuotaStatusResponse>(jsonString),
                APIResponseCode.DataQuotaExceeded => Deserialize<DataQuotaExceededResponse>(jsonString),
                APIResponseCode.AccessCheck => Deserialize<AccessCheckResponse>(jsonString),
                APIResponseCode.NewUserSuccess => Deserialize<NewUserSuccessResponse>(jsonString),
                APIResponseCode.NewUserFailure => Deserialize<NewUserFailureResponse>(jsonString),
                _ => throw new InvalidDataException("The given APIResponseCode is not supported"),
            };
    }

    public class BadDataKeyResponse : APIResponse
    {
        public string DataKey { get; set; }
        public string? Error { get; set; }
    }

    public class BadUserKeyResponse : APIResponse
    {
        public Guid Key { get; set; }
        public string? Error { get; set; }
    }

    public class UnspecifiedErrorResponse : APIResponse
    {
        public string? ErrorType { get; set; }
        public string? Message { get; set; }
    }

    public class TooManyRequestsResponse : APIResponse
    {
        public double Limit { get; set; }
        public TimeSpan Period { get; set; }
        public string RetryAfter { get; set; }
    }

    public class MessageResponse : APIResponse
    {
        public string MessageType { get; set; }
        public string MessageData { get; set; }
    }

    public class NewSessionSuccessResponse : APIResponse
    {
        public string Token { get; set; }
    }
    
    public class NewSessionFailureResponse : APIResponse
    {
        public string Reason { get; set; }
        public string Details { get; set; }
    }

    public class NewSessionBadRequestResponse : APIResponse
    {
        public string[] Reasons { get; set; }
    }

    public class RenewSessionSuccessResponse : APIResponse
    {
        public string Token { get; set; }
    }

    public class RenewSessionFailureResponse : APIResponse
    {
        public string Reason { get; set; }
    }

    public class RoleReportResponse : APIResponse
    {
        public string[] Roles { get; set; }
    }

    public class AuthStatusResponse : APIResponse
    {
        public bool IsAuthorized { get; set; }
        public bool IsRequestToken { get; set; }
    }

    public abstract class DataResponse : APIResponse
    {
        public string DataKey { get; set; }
    }

    public abstract class DataFailureResponse : DataResponse
    {
        public string Reason { get; set; }
    }

    public class DataUploadFailureResponse : DataFailureResponse { }
    public class DataDownloadFailureResponse : DataFailureResponse { }

    public class DataUploadSuccessResponse : DataResponse
    {
        public bool Overwritten { get; set; }
    }

    public class DataDownloadSuccessResponse : DataResponse
    {
        public byte[]? Data { get; set; }
    }

    public class TransferQuotaStatusResponse : APIResponse
    {
        public class TransferReport
        {
            public double Upload { get; set; }
            public double DOwnload { get; set; }
        }

        public TransferReport TransferUsage { get; set; }
        public TransferReport TransferQuota { get; set; }
        public double StorageUsage { get; set; }
        public double StorageQuota { get; set; }
    }

    public class DataQuotaExceededResponse : APIResponse
    {
        public double Excess { get; set; }
        public string Kind { get; set; }
    }

    public class AccessCheckResponse : APIResponse
    {
        public bool IsAccessible { get; set; }
    }

    public enum UserCreationResult
    {
        Accepted, AlreadyExists, Denied
    }
    
    public class ServiceData
    {
        public Service Service { get; set; }
        public object[] Data { get; set; }
    }
    public class UserCreationResults
    {
        public UserCreationResult? Result { get; set; }
        public Credentials Credentials { get; set; }
        public string? ReasonForDenial { get; set; }
        public ServiceData[] ServiceData { get; set; }
        public string[] ServiceConfigurationResults { get; set; }
    }

    public class NewUserSuccessResponse : APIResponse
    {
        public UserCreationResults Results { get; set; }
    }

    public class NewUserFailureResponse : APIResponse
    {
        public UserCreationResult? Results { get; set; }
    }
}
