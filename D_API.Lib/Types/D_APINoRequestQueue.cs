using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Lib.Types
{
    public class D_APINoRequestQueue : IRequestQueue
    {
        public Task<T> NewRequest<T>(Func<Task<T>> func) => func();

        public Task<T> NewRequest<T>(Func<T> func) => Task.Run(func);

        public Task NewRequest(Func<Task> func) => func();

        public Task NewRequest(Action func) => Task.Run(func);
    }
}
