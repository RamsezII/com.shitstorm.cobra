using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    public abstract class DevField
    {
        internal readonly string name;
        internal readonly Type ptype, type;

        internal static readonly Dictionary<Type, Dictionary<string, DevField>> all_fields = new();

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            all_fields.Clear();
        }

        //----------------------------------------------------------------------------------------------------------

        protected DevField(in string name, in Type ptype, in Type type)
        {
            this.name = name;
            this.ptype = ptype;
            this.type = type;
        }

        //----------------------------------------------------------------------------------------------------------

        internal protected abstract void OnTarget(in Janitor janitor, in object target);
    }

    public sealed class DevField<TClass, TAttr> : DevField
    {
        internal readonly Action<Janitor, TClass> onTarget;

        //----------------------------------------------------------------------------------------------------------

        public DevField(
            in string name,
            in Action<Janitor, TClass> onTarget
            ) : base(
                name: name,
                ptype: typeof(TClass),
                type: typeof(TAttr)
            )
        {
            this.onTarget = onTarget;
        }

        //----------------------------------------------------------------------------------------------------------

        public static void AddAttribute(in DevField<TClass, TAttr> attr)
        {
            if (all_fields.TryGetValue(typeof(TClass), out var attributes))
                attributes.Add(attr.name, attr);
            else
                all_fields.Add(typeof(TClass), new(StringComparer.Ordinal) { { attr.name, attr } });
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnTarget(in Janitor janitor, in object target)
        {
            onTarget(janitor, (TClass)target);
        }
    }
}