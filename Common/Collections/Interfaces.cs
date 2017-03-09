using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportRadar.Common.Collections
{
    public delegate int DelegateCompare<T>(T obj1, T obj2);
    public delegate bool DelegateForEach<T>(T item);

    public interface ISafelyForeach<T>
    {
        T SafelyForEach(DelegateForEach<T> dfe);
    }
}
