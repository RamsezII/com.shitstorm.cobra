using System;
using System.Collections.Generic;
using System.Linq;

namespace _COBRA_.Boa
{
    public sealed class MemScope
    {
        public readonly MemScope _parent;
        public readonly Dictionary<string, MemCell> _vars = new(StringComparer.Ordinal);

        //----------------------------------------------------------------------------------------------------------

        internal MemScope(in MemScope parent = null)
        {
            _parent = parent;
        }

        //----------------------------------------------------------------------------------------------------------

        public IEnumerable<string> EVarNames()
        {
            if (_parent != null)
                return _vars.Keys.Union(_parent.EVarNames());
            return _vars.Keys;
        }

        public bool TryGet(in string name, out MemCell value)
        {
            if (_vars.TryGetValue(name, out value))
                return true;
            else if (_parent != null)
                return _parent.TryGet(name, out value);
            return false;
        }

        public bool TrySet(in string name, in MemCell cell)
        {
            if (_vars.ContainsKey(name))
            {
                _vars[name] = new(cell);
                return true;
            }
            else if (_parent != null)
                return _parent.TrySet(name, cell);
            return false;
        }
    }
}