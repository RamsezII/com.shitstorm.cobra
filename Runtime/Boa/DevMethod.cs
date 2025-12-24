using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    /// <summary>
    /// instance and class methods
    /// </summary>
    public abstract class DevMethod
    {
        internal readonly string name;
        internal readonly Type output_type;
        internal readonly List<Type> targs;

        internal static readonly Dictionary<Type, Dictionary<string, DevMethod>> all_methods = new();

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            all_methods.Clear();
        }

        //----------------------------------------------------------------------------------------------------------

        protected DevMethod(in string name, in Type output_type, in List<Type> targs)
        {
            this.name = name;
            this.output_type = output_type;
            this.targs = targs;
        }

        //----------------------------------------------------------------------------------------------------------

        internal protected abstract void OnExecution(in Janitor janitor, in List<MemCell> args, in object target);
    }

    public sealed class DevMethod<TClass, TAttr> : DevMethod
    {
        internal readonly Action<Janitor, List<MemCell>, TClass> onExecution;

        //----------------------------------------------------------------------------------------------------------

        public DevMethod(
            in string name,
            in List<Type> targs,
            in Action<Janitor, List<MemCell>, TClass> onExecution
            ) : base(
                name: name,
                targs: targs,
                output_type: typeof(TAttr)
            )
        {
            this.onExecution = onExecution;
        }

        //----------------------------------------------------------------------------------------------------------

        public static void AddMethod(in DevMethod<TClass, TAttr> method)
        {
            if (all_methods.TryGetValue(typeof(TClass), out var methods))
                methods.Add(method.name, method);
            else
                all_methods.Add(typeof(TClass), new(StringComparer.Ordinal) { { method.name, method } });
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecution(in Janitor janitor, in List<MemCell> args, in object target)
        {
            onExecution(janitor, args, (TClass)target);
        }
    }
}