using System;
using System.Collections.Generic;
using System.Text;

namespace D_API.Lib.Types
{
    public class D_APIClientConfig
    {
        public readonly bool AutoQueue;

        public D_APIClientConfig(bool autoQueue = true)
        {
            AutoQueue = autoQueue;
        }
    }
}
