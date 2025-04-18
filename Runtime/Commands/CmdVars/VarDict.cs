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

        public void Dispose()
        {
            foreach (var value in Values)
                if (value is IDisposable disposable)
                    disposable.Dispose();
            Clear();
        }
    }
}