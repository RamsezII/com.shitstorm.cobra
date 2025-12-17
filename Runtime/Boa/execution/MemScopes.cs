using System;
using System.Collections.Generic;
using System.Linq;

namespace _COBRA_.Boa
{
    public sealed class MemScope
    {
        internal readonly MemScope parent;
        internal readonly Dictionary<string, MemCell> _vars = new(StringComparer.Ordinal);

        //----------------------------------------------------------------------------------------------------------

        internal MemScope(in MemScope parent = null)
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

        public bool TryGet(in string name, out MemCell value)
        {
            if (_vars.TryGetValue(name, out value))
                return true;
            else if (parent != null)
                return parent.TryGet(name, out value);
            return false;
        }

        public bool TrySet(in string name, in MemCell cell)
        {
            if (_vars.ContainsKey(name))
            {
                _vars[name] = new(cell);
                return true;
            }
            else if (parent != null)
                return parent.TrySet(name, cell);
            return false;
        }
    }
}