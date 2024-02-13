using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTVRelay.Library
{
    public class InlineEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equalsFunc;
        private readonly Func<T, int> _hashCodeFunc;

        public InlineEqualityComparer(Func<T, T, bool> equalsFunc, Func<T, int> hashCodeFunc)
        {
            _equalsFunc = equalsFunc ?? throw new ArgumentNullException(nameof(equalsFunc));
            _hashCodeFunc = hashCodeFunc ?? throw new ArgumentNullException(nameof(hashCodeFunc));
        }

        public bool Equals(T x, T y)
        {
            return _equalsFunc(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _hashCodeFunc(obj);
        }
    }
}
