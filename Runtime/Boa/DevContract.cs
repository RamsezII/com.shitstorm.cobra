using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa
{
    public sealed class DevContract
    {
        public readonly struct OptionKey
        {
            public readonly char short_name;
            public readonly string long_name;

            //----------------------------------------------------------------------------------------------------------

            public OptionKey(in char short_name, in string long_name)
            {
                this.short_name = short_name;
                this.long_name = long_name;
            }

            //----------------------------------------------------------------------------------------------------------

            public override string ToString()
            {
                if (short_name == 0)
                    if (string.IsNullOrEmpty(long_name))
                        return "--???";
                    else
                        return $"--{long_name}";
                return $"-{short_name}/--{long_name}";
            }
        }

        public class VOptions : Dictionary<OptionKey, MemCell>
        {
        }

        public class VArguments : List<MemCell>
        {
        }

        public struct Parameters
        {
            public Janitor janitor;
            public VOptions options;
            public VArguments arguments;

            //----------------------------------------------------------------------------------------------------------

            public Parameters(in Janitor janitor, in VOptions options, in VArguments arguments)
            {
                this.janitor = janitor;
                this.options = options;
                this.arguments = arguments;
            }
        }

        public readonly string name;
        public readonly Type output_type;
        internal readonly Dictionary<OptionKey, Type> options;
        internal readonly List<Type> arguments;
        internal readonly Action<Janitor, Parameters> action_SIG_EXE;
        internal readonly Func<Janitor, Parameters, IEnumerator<ExecutionOutput>> routine_SIG_EXE, routine_SIG_READER;

        internal static readonly Dictionary<string, DevContract> contracts = new(StringComparer.OrdinalIgnoreCase);

        //----------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            contracts.Clear();
        }

        //----------------------------------------------------------------------------------------------------------

        public static void AddContract(in DevContract contract, params string[] aliases)
        {
            contracts.Add(contract.name, contract);
            for (int i = 0; i < aliases.Length; i++)
                contracts.Add(aliases[i], contract);
        }

        //----------------------------------------------------------------------------------------------------------

        public DevContract(
            in string name,
            in Type output_type,
            in Dictionary<OptionKey, Type> options = null,
            in List<Type> arguments = null,
            in Action<Janitor, Parameters> action_SIG_EXE = null,
            in Func<Janitor, Parameters, IEnumerator<ExecutionOutput>> routine_SIG_EXE = null,
            in Func<Janitor, Parameters, IEnumerator<ExecutionOutput>> routine_SIG_READER = null
            )
        {
            this.name = name;
            this.output_type = output_type == typeof(int) || output_type == typeof(float) ? typeof(BoaNumber) : output_type;
            this.options = options;
            this.arguments = arguments;
            this.action_SIG_EXE = action_SIG_EXE;
            this.routine_SIG_EXE = routine_SIG_EXE;
            this.routine_SIG_READER = routine_SIG_READER;
        }
    }
}