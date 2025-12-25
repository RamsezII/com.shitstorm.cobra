using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    public abstract class DevField
    {
        internal readonly string name;
        internal readonly Type output_type;

        internal static readonly Dictionary<Type, Dictionary<string, DevField>> all_fields = new();

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            all_fields.Clear();
        }

        //----------------------------------------------------------------------------------------------------------

        protected DevField(in string name, in Type output_type)
        {
            this.name = name;
            this.output_type = output_type;
        }

        //----------------------------------------------------------------------------------------------------------

        internal protected abstract void OnExecution(in MemStack memstack, in MemScope memscope, in object target);
    }

    public sealed class DevField<TClass, TAttr> : DevField
    {
        internal readonly Action<MemStack, MemScope, TClass> onExecution;

        //----------------------------------------------------------------------------------------------------------

        public DevField(
            in string name,
            in Action<MemStack, MemScope, TClass> onExecution
            ) : base(
                name: name,
                output_type: typeof(TAttr)
            )
        {
            this.onExecution = onExecution;
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

        protected internal override void OnExecution(in MemStack memstack, in MemScope memscope, in object target)
        {
            onExecution(memstack, memscope, (TClass)target);
        }
    }
}