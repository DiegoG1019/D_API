using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Lib.Types
{
    public interface IRequestQueue
    {
        Task<T> NewRequest<T>(Func<Task<T>> func, Endpoint endpoint);
        Task<T> NewRequest<T>(Func<T> func, Endpoint endpoint);
        Task NewRequest(Func<Task> func, Endpoint endpoint);
        Task NewRequest(Action func, Endpoint endpoint);
    }
}
