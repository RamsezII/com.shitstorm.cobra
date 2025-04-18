using _UTIL_;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public sealed partial class Command
    {
        public static readonly Command static_domain = new(nameof(static_domain));

        public readonly string name;
        public readonly Action<Executor> action;
        public readonly Action<Executor, object> on_pipe;
        public readonly Func<Executor, IEnumerator<CMD_STATUS>> routine;

        public readonly Traductions manual;
        public readonly bool immortal, no_background;
        public readonly int min_args, max_args;
        public readonly Action<Executor> opts, args;
        public readonly Type input_type, output_type;

        public bool IsDomain => _commands.Count >= 0;
        public bool IsDomain_old => _commands.Count >= 0 || action == null && on_pipe == null && routine == null;

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            static_domain.PropagateOblivion();
        }

        //--------------------------------------------------------------------------------------------------------------

        internal Command(
            in string name,
            in Action<Executor> action = default,
            in Action<Executor, object> on_pipe = default,
            in Func<Executor, IEnumerator<CMD_STATUS>> routine = default,
            in Traductions manual = default,
            in bool immortal = default,
            in bool no_background = default,
            in int min_args = default,
            in int max_args = default,
            in Type input_type = default,
            in Type output_type = default,
            in Action<Executor> opts = default,
            in Action<Executor> args = default)
        {
            this.name = name;
            this.action = action;
            this.on_pipe = on_pipe;
            this.input_type = input_type ?? typeof(object);
            this.output_type = output_type ?? typeof(object);
            this.routine = routine;
            this.manual = manual;
            this.immortal = immortal;
            this.no_background = no_background;
            this.min_args = min_args;
            this.max_args = Mathf.Max(min_args, max_args);
            this.opts = opts;
            this.args = args;

            if (this.max_args == 0 && args != null)
                throw new Exception($"'{name}' assigned {nameof(this.args)} callback but {nameof(this.max_args)} is 0");
        }

        public Command AddCommand(in Command command, params string[] aliases)
        {
            _commands.Add(command.name, command);
            for (int i = 0; i < aliases.Length; ++i)
                _commands.Add(aliases[i], command);
            return command;
        }

        public Command AddDomain(
            in string name,
            in Traductions manual = default,
            params string[] aliases
            ) => AddCommand(new(name, manual: manual), aliases);

        public Command AddAction(
            in string name,
            in Action<Executor> action,
            in Action<Executor> opts = default,
            in Action<Executor> args = default,
            in Type output_type = default,
            in Traductions manual = default,
            in bool background = default,
            in bool no_background = default,
            in int min_args = default,
            in int max_args = default,
            params string[] aliases
            ) => AddCommand(new(name,
                                action: action,
                                manual: manual,
                                no_background: no_background,
                                min_args: min_args,
                                max_args: max_args,
                                output_type: output_type,
                                opts: opts,
                                args: args), aliases);

        public Command AddPipe(
            in string name,
            in Action<Executor, object> on_pipe,
            in Action<Executor> opts = null,
            in Action<Executor> args = null,
            in Type input_type = null,
            in Type output_type = null,
            in Traductions manual = default,
            in bool background = default,
            in bool no_background = default,
            in int min_args = default,
            in int max_args = default,
            params string[] aliases
            ) => AddCommand(new(name,
                                manual: manual,
                                no_background: no_background,
                                opts: opts,
                                args: args,
                                min_args: min_args,
                                max_args: max_args,
                                input_type: input_type,
                                output_type: output_type,
                                on_pipe: on_pipe), aliases);

        public Command AddRoutine(
            in string name,
            in Func<Executor, IEnumerator<CMD_STATUS>> routine,
            in bool immortal = default,
            in Action<Executor> opts = null,
            in Action<Executor> args = null,
            in Type output_type = null,
            in Traductions manual = default,
            in bool background = default,
            in bool no_background = default,
            in int min_args = default,
            in int max_args = default,
            params string[] aliases
            ) => AddCommand(new(name,
                                routine: routine,
                                manual: manual,
                                no_background: no_background,
                                min_args: min_args,
                                max_args: max_args,
                                immortal: immortal,
                                output_type: output_type,
                                opts: opts,
                                args: args), aliases);
    }
}