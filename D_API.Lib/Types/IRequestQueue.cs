using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Lib.Types
{
    public interface IRequestQueue
    {
        Task<T> NewRequest<T>(Func<Task<T>> func);
        Task<T> NewRequest<T>(Func<T> func);
        Task NewRequest(Func<Task> func);
        Task NewRequest(Action func);
    }
}
