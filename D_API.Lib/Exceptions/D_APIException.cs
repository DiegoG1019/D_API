using System;
using System.Collections.Generic;
using System.Text;

namespace D_API.Lib.Exceptions
{
    [Serializable]
    public class D_APIException : Exception
    {
        public D_APIException() { }
        public D_APIException(string message) : base(message) { }
        public D_APIException(string message, Exception inner) : base(message, inner) { }
        protected D_APIException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class D_APINoDataException : D_APIException
    {
        public D_APINoDataException() { }
        public D_APINoDataException(string message) : base(message) { }
        public D_APINoDataException(string message, Exception inner) : base(message, inner) { }
        protected D_APINoDataException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class D_APIUnauthorizedLoginException : D_APIException
    {
        public D_APIUnauthorizedLoginException() { }
        public D_APIUnauthorizedLoginException(string message) : base(message) { }
        public D_APIUnauthorizedLoginException(string message, Exception inner) : base(message, inner) { }
        protected D_APIUnauthorizedLoginException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    
    [Serializable]
    public class D_APIUnauthorizedDataAccessException : D_APIException
    {
        public D_APIUnauthorizedDataAccessException() { }
        public D_APIUnauthorizedDataAccessException(string message) : base(message) { }
        public D_APIUnauthorizedDataAccessException(string message, Exception inner) : base(message, inner) { }
        protected D_APIUnauthorizedDataAccessException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class D_APITooManyRequestsException : D_APIException
    {
        public D_APITooManyRequestsException() { }
        public D_APITooManyRequestsException(string message) : base(message) { }
        public D_APITooManyRequestsException(string message, Exception inner) : base(message, inner) { }
        protected D_APITooManyRequestsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class D_APIRequestJWTExpiredException : D_APIException
    {
        public D_APIRequestJWTExpiredException() { }
        public D_APIRequestJWTExpiredException(string message) : base(message) { }
        public D_APIRequestJWTExpiredException(string message, Exception inner) : base(message, inner) { }
        protected D_APIRequestJWTExpiredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
