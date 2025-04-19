using System;
using System.Collections.Generic;

namespace _COBRA_
{
    public sealed class VarDict : Dictionary<string, object>, IDisposable
    {
        public VarDict() : base(StringComparer.Ordinal)
        {
        }

        //--------------------------------------------------------------------------------------------------------------

        public bool TryGetValue_str(in string key, out string value)
        {
            if (TryGetValue(key, out object o))
            {
                value = o.ToString();
                return true;
            }
            value = null;
            return false;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            foreach (var value in Values)
                if (value is IDisposable disposable)
                    disposable.Dispose();
            Clear();
        }
    }
}