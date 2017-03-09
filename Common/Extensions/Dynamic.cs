using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Extensions
{
    public static class Dynamic
    {
        public static bool PropertyExists(dynamic dyn, string name)
        {
            return dyn.GetType().GetProperty(name) != null;
        }
    }
}
