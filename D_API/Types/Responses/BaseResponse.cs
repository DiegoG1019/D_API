﻿using D_API.Enums;
using D_API.Types.Auth;
using D_API.Types.DataKeeper;
using D_API.Types.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace D_API.Types.Responses
{
    public enum APIResponseCode : ushort
    {
        TooManyRequests = 1, BadUserKey = 2, NotInSession = 3, UnspecifiedError = 4,

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

    public abstract record BaseResponse(APIResponseCode APIResponseCode, string Title);

    public record UnspecifiedError(string Title, string? ErrorType = null, string? Message = null) : BaseResponse(APIResponseCode.UnspecifiedError, Title);
    public record TooManyRequests(double Limit, TimeSpan Period, string RetryAfter, string Title = "You have exceeded your request limit") : BaseResponse(APIResponseCode.TooManyRequests, Title);
    public record BadUserKey(Guid Key, string? Error) : BaseResponse(APIResponseCode.BadUserKey, "The user ID is invalid");
    public record NotInSession(string? Title = null) : BaseResponse(APIResponseCode.NotInSession, Title ?? "This request does not have a valid Session or Request token");

    // ----

    public record Message(string Title, string MessageType, string MessageData) : BaseResponse(APIResponseCode.Message, Title);

    // ----

    public record NewSessionSuccess(string Token) : BaseResponse(APIResponseCode.NewSessionSuccess, "Opened a new session");
    public record NewSessionFailure(string Reason, string Details) : BaseResponse(APIResponseCode.NewSessionFailure, Reason) 
    {
        public NewSessionFailure(CredentialVerificationResult Reason, string Details) : this(Reason.ToString(), Details) { }
    }
    public record NewSessionBadRequest(IEnumerable<string> Reasons) : BaseResponse(APIResponseCode.NewSessionBadRequest, "Could not validate New Session Request");

    public record RenewSessionSuccess(string Token) : BaseResponse(APIResponseCode.RenewSessionSuccess, "Renewed Session Request Token");
    public record RenewSessionFailure(string Reason) : BaseResponse(APIResponseCode.RenewSessionFailure, Reason);
    
    public record RoleReport(params string[] Roles) : BaseResponse(APIResponseCode.RoleReport, "Roles");

    public record AuthStatus(bool IsAuthorized, bool IsRequestToken) : BaseResponse(APIResponseCode.AuthStatus, "Authorization Status");

    // ----

    public record DataUploadSuccess(string DataKey, bool Overwritten) : BaseResponse(APIResponseCode.DataUploadSuccess, "Uploaded Data");
    public record DataUploadFailure(string DataKey, string Reason) : BaseResponse(APIResponseCode.DataUploadFailure, Reason);

    public record DataDownloadSuccess(string DataKey, byte[]? Data) : BaseResponse(APIResponseCode.DataDownloadSuccess, "Downloaded Data");
    public record DataDownloadFailure(string DataKey, string Reason) : BaseResponse(APIResponseCode.DataDownloadFailure, Reason);

    public record TransferQuotaStatus(TransferReport TransferUsage, TransferReport TransferQuota, double StorageUsage, double StorageQuota) : BaseResponse(APIResponseCode.TransferQuotaStatus, "Transfer Report");
    public record DataQuotaExceeded(double Excess, string Kind) : BaseResponse(APIResponseCode.DataQuotaExceeded, $"Exceeded {Kind} Quota");

    public record BadDataKey(string DataKey, string? Error) : BaseResponse(APIResponseCode.BadDataKey, "Data Key is invalid");

    public record AccessCheck(bool IsAccesible) : BaseResponse(APIResponseCode.AccessCheck, "Access Check");

    // ----

    public record NewUserSuccess(UserCreationResults Results) : BaseResponse(APIResponseCode.NewUserSuccess, "Created new User Account");
    public record NewUserFailure(string Title, UserCreationResults? Results = null) : BaseResponse(APIResponseCode.NewUserFailure, Title);
}
