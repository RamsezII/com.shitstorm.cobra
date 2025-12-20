using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    public class DevType
    {
        internal readonly Type target_type, accepted_type;
        internal readonly Color lint;
        public readonly Dictionary<string, object> candidates;

        //----------------------------------------------------------------------------------------------------------

        protected DevType(in Type target_type, in Type accepted_type, in Color lint, in Dictionary<string, object> candidates = null)
        {
            this.target_type = target_type;
            this.accepted_type = accepted_type;
            this.lint = lint;
            this.candidates = candidates ?? new();
        }
    }

    public class DevType<TargetType, AcceptedType> : DevType
    {
        public DevType(in Color lint, in Dictionary<string, object> candidates = null) : base(typeof(TargetType), typeof(AcceptedType), lint, candidates)
        {
        }
    }

    public static class DevTypes
    {
        static readonly Dictionary<Type, DevType> all_types = new();

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            all_types.Clear();
        }

        //----------------------------------------------------------------------------------------------------------

        public static DevType AddType(in DevType dtype)
        {
            all_types.Add(dtype.target_type, dtype);
            return dtype;
        }

        //----------------------------------------------------------------------------------------------------------

        internal static bool TryDevType(in CodeReader reader, in Type expected_type, out AstExpression ast_literal)
        {
            if (all_types.TryGetValue(expected_type, out var devtype))
                if (reader.TryReadString_matches_out(out string match, false, devtype.lint, devtype.candidates.Keys, stoppers: " \n\r[]{}(),;'\"\\=-*/%<>|&"))
                //if (reader.TryLonguestMatch(out string match, devtype.lint, true, devtype.candidates.Keys))
                {
                    object value = devtype.candidates[match];
                    ast_literal = new AstLiteral(value, devtype.target_type);
                    return true;
                }
            ast_literal = null;
            return false;
        }
    }
}