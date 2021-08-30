using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace D_API.Lib.Types
{
    public class D_APIRequestQueue : IRequestQueue
    {
        public Task<T> NewRequest<T>(Func<Task<T>> func)
        {

        }

        public Task<T> NewRequest<T>(Func<T> func)
        {
            throw new NotImplementedException();
        }

        public Task NewRequest(Func<Task> func)
        {
            throw new NotImplementedException();
        }

        public Task NewRequest(Action func)
        {
            throw new NotImplementedException();
        }
    }
}
