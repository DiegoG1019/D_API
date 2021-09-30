using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Enums
{
    [Flags]
    public enum Service
    {
        Users = 1 << 0,
        Authorization = 1 << 1,
        Data = 1 << 2,
    }

}
