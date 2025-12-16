using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    public sealed class DevAttribute
    {
        public readonly string name;
        public readonly Type output_type;

        internal static readonly Dictionary<Type, Dictionary<string, DevAttribute>> _attributes = new();

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _attributes.Clear();
        }

        //----------------------------------------------------------------------------------------------------------

        public static void AddAttribute(in Type dev_type, in string attr_name, in Type attr_type)
        {
            if (_attributes.TryGetValue(dev_type, out var attributes))
                attributes.Add(attr_name, new(attr_name, attr_type));
            else
                _attributes.Add(dev_type, new(StringComparer.Ordinal)
                {
                    {
                        attr_name,
                        new(attr_name, attr_type)
                    },
                });
        }

        //----------------------------------------------------------------------------------------------------------

        public DevAttribute(
            in string name,
            in Type output_type
            )
        {
            this.name = name;
            this.output_type = output_type;
        }
    }
}