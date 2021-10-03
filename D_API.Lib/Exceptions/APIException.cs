using D_API.Lib.Models.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace D_API.Lib.Exceptions
{
    [Serializable]
    public abstract class APIException : Exception
    {
        public APIResponse Response { get; set; }
        public APIException(APIResponse response) : base($"{(int)response.APIResponseCode}::{response.Title}\n{response.Serialize()}") => Response = response;

        public static APIException GetException(string jsonString)
            => APIResponse.GetResponseCode(jsonString) switch
            {
                APIResponseCode.BadDataKey => new BadDataKeyException(APIResponse.GetResponse<BadDataKeyResponse>(jsonString)),
                APIResponseCode.UnspecifiedError => new UnspecifiedErrorException(APIResponse.GetResponse<UnspecifiedErrorResponse>(jsonString)),
                APIResponseCode.TooManyRequests => new TooManyRequestsException(APIResponse.GetResponse<TooManyRequestsResponse>(jsonString)),
                APIResponseCode.BadUserKey => new BadUserKeyException(APIResponse.GetResponse<BadUserKeyResponse>(jsonString)),
                APIResponseCode.NewSessionFailure => new NewSessionFailureException(APIResponse.GetResponse<NewSessionFailureResponse>(jsonString)),
                APIResponseCode.NewSessionBadRequest => new NewSessionBadRequestException(APIResponse.GetResponse<NewSessionBadRequestResponse>(jsonString)),
                APIResponseCode.RenewSessionFailure => new RenewSessionFailureException(APIResponse.GetResponse<RenewSessionFailureResponse>(jsonString)),
                APIResponseCode.DataUploadFailure => new DataUploadFailureException(APIResponse.GetResponse<DataUploadFailureResponse>(jsonString)),
                APIResponseCode.DataDownloadFailure => new DataDownloadFailureException(APIResponse.GetResponse<DataDownloadFailureResponse>(jsonString)),
                APIResponseCode.DataQuotaExceeded => new DataQuotaExceededException(APIResponse.GetResponse<DataQuotaExceededResponse>(jsonString)),
                APIResponseCode.NewUserFailure => new NewUserFailureException(APIResponse.GetResponse<NewUserFailureResponse>(jsonString)),
                _ => throw new InvalidDataException("The given APIResponseCode is not supported as an Exception"),
            };

        public static APIException GetException(APIResponse response)
            => response.APIResponseCode switch
            {
                APIResponseCode.BadDataKey => new BadDataKeyException((BadDataKeyResponse)response),
                APIResponseCode.UnspecifiedError => new UnspecifiedErrorException((UnspecifiedErrorResponse)response),
                APIResponseCode.TooManyRequests => new TooManyRequestsException((TooManyRequestsResponse)response),
                APIResponseCode.BadUserKey => new BadUserKeyException((BadUserKeyResponse)response),
                APIResponseCode.NewSessionFailure => new NewSessionFailureException((NewSessionFailureResponse)response),
                APIResponseCode.NewSessionBadRequest => new NewSessionBadRequestException((NewSessionBadRequestResponse)response),
                APIResponseCode.RenewSessionFailure => new RenewSessionFailureException((RenewSessionFailureResponse)response),
                APIResponseCode.DataUploadFailure => new DataUploadFailureException((DataUploadFailureResponse)response),
                APIResponseCode.DataDownloadFailure => new DataDownloadFailureException((DataDownloadFailureResponse)response),
                APIResponseCode.DataQuotaExceeded => new DataQuotaExceededException((DataQuotaExceededResponse)response),
                APIResponseCode.NewUserFailure => new NewUserFailureException((NewUserFailureResponse)response),
                _ => throw new InvalidDataException("The given APIResponseCode is not supported as an Exception"),
            };
    }

    [Serializable]
    public class TooManyRequestsException : APIException
    {
        public TooManyRequestsException(TooManyRequestsResponse response) : base(response) { }
    }

    [Serializable]
    public class NewSessionFailureException : APIException
    {
        public NewSessionFailureException(NewSessionFailureResponse response) : base(response) { }
    }

    [Serializable]
    public class RenewSessionFailureException : APIException
    {
        public RenewSessionFailureException(RenewSessionFailureResponse response) : base(response) { }
    }

    [Serializable]
    public class NewSessionBadRequestException : APIException
    {
        public NewSessionBadRequestException(NewSessionBadRequestResponse response) : base(response) { }
    }

    [Serializable]
    public class UnspecifiedErrorException : APIException
    {
        public UnspecifiedErrorException(UnspecifiedErrorResponse response) : base(response) { }
    }

    [Serializable]
    public class DataUploadFailureException : APIException
    {
        public DataUploadFailureException(DataUploadFailureResponse response) : base(response) { }
    }

    [Serializable]
    public class DataDownloadFailureException : APIException
    {
        public DataDownloadFailureException(DataDownloadFailureResponse response) : base(response) { }
    }

    [Serializable]
    public class DataQuotaExceededException : APIException
    {
        public DataQuotaExceededException(DataQuotaExceededResponse response) : base(response) { }
    }

    [Serializable]
    public class BadUserKeyException : APIException
    {
        public BadUserKeyException(BadUserKeyResponse response) : base(response) { }
    }

    [Serializable]
    public class BadDataKeyException : APIException
    {
        public BadDataKeyException(BadDataKeyResponse response) : base(response) { }
    }

    [Serializable]
    public class NewUserFailureException : APIException
    {
        public NewUserFailureException(NewUserFailureResponse response) : base(response) { }
    }
}
