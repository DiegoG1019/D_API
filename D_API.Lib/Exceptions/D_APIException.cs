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
}
