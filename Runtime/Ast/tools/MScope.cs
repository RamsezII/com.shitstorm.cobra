using System;
using System.Collections.Generic;
using System.Linq;

namespace _COBRA_.Boa
{
    public abstract class MScope<T>
    {
        internal readonly MScope<T> parent;
        internal readonly Dictionary<string, T> _vars = new(StringComparer.Ordinal);

        //----------------------------------------------------------------------------------------------------------

        internal MScope(in MScope<T> parent)
        {
            this.parent = parent;
        }

        //----------------------------------------------------------------------------------------------------------

        public IEnumerable<string> EVarNames()
        {
            if (parent != null)
                return _vars.Keys.Union(parent.EVarNames());
            return _vars.Keys;
        }

        public bool TryGet(in string name, out T value)
        {
            if (_vars.TryGetValue(name, out value))
                return true;
            else if (parent != null)
                return parent.TryGet(name, out value);
            return false;
        }
    }
}