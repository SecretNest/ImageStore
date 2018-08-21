using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretNest.ImageStore
{
    [Flags]
    public enum StringPropertyComparingModes : int
    {
        Equals = 1,
        Contains = 2,
        StartsWith = 4,
        EndsWith = 8
    }
}
