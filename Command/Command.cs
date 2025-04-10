using _UTIL_;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public sealed partial class Command
    {
        public readonly string name;
        public readonly Traductions manual;
        public readonly int min_args, max_args;
        public readonly Action<Executor> args;

        public readonly Action<Executor, List<object>, object> on_pipe;
        public readonly Func<Executor, IEnumerator<CMD_STATUS>> routine;
        public readonly Action<Executor> action;

        public bool IsDomain => _commands.Count >= 0 || action == null && on_pipe == null && routine == null;

        //--------------------------------------------------------------------------------------------------------------

        internal Command(
            in string name,
            in Traductions manual = default,
            in int min_args = default,
            in int max_args = default,
            in Action<Executor> args = default,
            in Action<Executor> action = default,
            in Action<Executor, List<object>, object> on_pipe = default,
            in Func<Executor, IEnumerator<CMD_STATUS>> routine = default
            )
        {
            this.name = name;
            this.manual = manual;
            this.min_args = min_args;
            this.max_args = Mathf.Max(min_args, max_args);
            this.args = args;
            this.action = action;
            this.on_pipe = on_pipe;
            this.routine = routine;
        }

        //--------------------------------------------------------------------------------------------------------------

        private Command AddCommand(in Command command, params string[] aliases)
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
            ) => AddCommand(new(name, manual), aliases);

        public Command AddAction(
            in string name,
            in Action<Executor> args,
            in Action<Executor> action,
            in Traductions manual = default,
            in int min_args = default,
            in int max_args = default,
            params string[] aliases
            ) => AddCommand(new(name, manual, min_args, max_args, args, action: action), aliases);

        public Command AddPipe(
            in string name,
            in Action<Executor> args,
            in Action<Executor, List<object>, object> on_pipe,
            in Traductions manual = default,
            in int min_args = default,
            in int max_args = default,
            params string[] aliases
            ) => AddCommand(new(name, manual, min_args, max_args, args, on_pipe: on_pipe), aliases);

        public Command AddRoutine(
            in string name,
            in Action<Executor> args,
            in Func<Executor, IEnumerator<CMD_STATUS>> routine,
            in Traductions manual = default,
            in int min_args = default,
            in int max_args = default,
            params string[] aliases
            ) => AddCommand(new(name, manual, min_args, max_args, args, routine: routine), aliases);
    }
}