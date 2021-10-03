using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Lib.Internal
{
    internal class D_APINoRequestQueue : IRequestQueue
    {
        public Task<T> NewRequest<T>(Func<Task<T>> func, Endpoint _) => func();

        public Task<T> NewRequest<T>(Func<T> func, Endpoint _) => Task.Run(func);

        public Task NewRequest(Func<Task> func, Endpoint _) => func();

        public Task NewRequest(Action func, Endpoint _) => Task.Run(func);
    }
}
