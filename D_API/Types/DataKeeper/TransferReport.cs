using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Types.DataKeeper
{
    public record TransferReport(double Upload, double Download)
    {
        public TransferReport((double up, double down) tuple) : this(tuple.up, tuple.down) { }
    }
}
