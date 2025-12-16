using System;

namespace _COBRA_.Boa.compilation
{
    public sealed class TScope : MScope<Type>
    {

        //----------------------------------------------------------------------------------------------------------

        internal TScope(in TScope parent) : base(parent)
        {
        }
    }
}