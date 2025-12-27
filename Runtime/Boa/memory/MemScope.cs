using _ARK_;
using _UTIL_;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _COBRA_.Boa
{
    public sealed class MemScope : Disposable
    {
        public readonly BoaShell shell;
        public readonly MemScope _parent;
        static readonly Dictionary<string, MemCell> _svars = new(StringComparer.Ordinal);
        public readonly Dictionary<string, MemCell> _vars = new(StringComparer.Ordinal);
        public readonly Dictionary<string, MemMethod> _methods = new(StringComparer.Ordinal);
        public MemScope GetSubScope(in string name) => new(name, this);

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _svars.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            NUCLEOR.delegates.Update_OnShellTick_before += static () =>
            {
                _svars["_time"] = Time.time;
                _svars["_ftime"] = Time.fixedTime;
                _svars["_dtime"] = Time.deltaTime;
                _svars["_frame"] = Time.frameCount;
                _svars["_fframe"] = NUCLEOR.instance.fixedFrameCount;
            };
        }

        //----------------------------------------------------------------------------------------------------------

        internal MemScope(in string name, in BoaShell shell) : base($"{shell.name}->{name}")
        {
            this.shell = shell;
        }

        MemScope(in string name, in MemScope parent) : base($"{parent.name}->{name}")
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
            IEnumerable<string> names = _svars.Keys.Union(_vars.Keys);
            if (_parent != null)
                return names.Union(_parent.EVarNames());
            return names;
        }

        public IEnumerable<string> EMetNames()
        {
            if (_parent != null)
                return _methods.Keys.Union(_parent.EMetNames());
            return _methods.Keys;
        }

        public bool TryGetVariable(in string name, out MemCell value, out MemScope scope)
        {
            if (_svars.TryGetValue(name, out value))
            {
                scope = null;
                return true;
            }
            else if (_vars.TryGetValue(name, out value))
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
                if (scope == null)
                    Debug.LogWarning($"tried setting read-only var: '{name}'");
                else
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