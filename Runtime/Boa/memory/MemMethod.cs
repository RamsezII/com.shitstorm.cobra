using System;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    public readonly struct MemMethod
    {
        public readonly struct OptionKey
        {
            public readonly char short_name;
            public readonly string long_name;
            public OptionKey(in char short_name, in string long_name)
            {
                this.short_name = short_name;
                this.long_name = long_name;
            }
        }

        public readonly struct TArgument
        {
            public readonly string name;
            public readonly Type type;
            public TArgument(in string name, in Type type)
            {
                this.name = name;
                this.type = type;
            }
        }

        public readonly string name;
        public readonly MemScope scope;
        public readonly Dictionary<OptionKey, Type> topts;
        public readonly List<TArgument> targs;
        public readonly AstStatement ast;
        public readonly Type output_type;

        //----------------------------------------------------------------------------------------------------------

        internal MemMethod(in string name, in MemScope scope, in AstStatement ast, in Type output_type)
        {
            this.name = name;
            this.scope = scope;
            topts = new();
            targs = new();
            this.ast = ast;
            this.output_type = output_type;
        }
    }
}