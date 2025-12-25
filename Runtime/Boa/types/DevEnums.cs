using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    public class DevEnum
    {
        internal readonly Type target_type, accepted_type;
        internal readonly Color lint;
        public readonly Dictionary<string, object> candidates;

        //----------------------------------------------------------------------------------------------------------

        protected DevEnum(in Type target_type, in Type accepted_type, in Color lint, in Dictionary<string, object> candidates = null)
        {
            this.target_type = target_type;
            this.accepted_type = accepted_type;
            this.lint = lint;
            this.candidates = candidates ?? new();
        }
    }

    public class DevType<TargetType, AcceptedType> : DevEnum
    {
        public DevType(in Color lint, in Dictionary<string, object> candidates = null) : base(typeof(TargetType), typeof(AcceptedType), lint, candidates)
        {
        }
    }

    public static class DevEnums
    {
        static readonly Dictionary<Type, DevEnum> all_enums = new();

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            all_enums.Clear();
        }

        //----------------------------------------------------------------------------------------------------------

        public static DevEnum AddEnum(in DevEnum dev_enum)
        {
            all_enums.Add(dev_enum.target_type, dev_enum);
            return dev_enum;
        }

        //----------------------------------------------------------------------------------------------------------

        internal static bool TryDevType(in CodeReader reader, in Type expected_type, out AstExpression ast_literal)
        {
            if (all_enums.TryGetValue(expected_type, out var dev_enum))
                //if (reader.TryReadString_matches_out(out string match, false, devtype.lint, devtype.candidates.Keys, stoppers: " \n\r[]{}(),;'\"\\=-*/%<>|&"))
                if (reader.TryMatchLonguestCandidate(out string match, true, dev_enum.lint, dev_enum.candidates.Keys, stops: string.Empty))
                {
                    object value = dev_enum.candidates[match];
                    ast_literal = new AstLiteral(value, dev_enum.target_type);
                    return true;
                }
            ast_literal = null;
            return false;
        }
    }
}