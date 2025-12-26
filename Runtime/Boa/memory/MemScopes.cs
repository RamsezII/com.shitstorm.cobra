using System;
using System.Collections.Generic;
using System.Linq;

namespace _COBRA_.Boa
{
    public sealed class MemScope
    {
        internal readonly BoaShell shell;
        public readonly MemScope _parent;
        public readonly Dictionary<string, MemCell> _vars = new(StringComparer.Ordinal);
        public readonly Dictionary<string, MemMethod> _methods = new(StringComparer.Ordinal);
        public MemScope GetSubScope() => new(this);

        //----------------------------------------------------------------------------------------------------------

        internal MemScope(in BoaShell shell)
        {
            this.shell = shell;
        }

        MemScope(in MemScope parent)
        {
            shell = parent.shell;
            _parent = parent;
        }

        //----------------------------------------------------------------------------------------------------------

        public void PropagateAmnesia()
        {
            _vars.Clear();
            _methods.Clear();
        }

        public IEnumerable<string> EVarNames()
        {
            if (_parent != null)
                return _vars.Keys.Union(_parent.EVarNames());
            return _vars.Keys;
        }

        public IEnumerable<string> EMetNames()
        {
            if (_parent != null)
                return _methods.Keys.Union(_parent.EMetNames());
            return _methods.Keys;
        }

        public bool TryGetVariable(in string name, out MemCell value, out MemScope scope)
        {
            if (_vars.TryGetValue(name, out value))
            {
                scope = this;
                return true;
            }
            else if (_parent != null)
                return _parent.TryGetVariable(name, out value, out scope);
            scope = null;
            return false;
        }

        public bool TryGetMethod(in string name, out MemMethod value, out MemScope scope)
        {
            if (_methods.TryGetValue(name, out value))
            {
                scope = this;
                return true;
            }
            else if (_parent != null)
                return _parent.TryGetMethod(name, out value, out scope);
            scope = null;
            return false;
        }

        public bool TrySetVariable(in string name, in MemCell cell)
        {
            if (TryGetVariable(name, out _, out var scope))
            {
                scope._vars[name] = cell;
                return true;
            }
            _vars[name] = cell;
            return true;
        }

        internal bool TrySetMethod(in string name, in MemMethod method)
        {
            if (TryGetMethod(name, out _, out var scope))
            {
                scope._methods[name] = method;
                return true;
            }
            _methods[name] = method;
            return true;
        }
    }
}