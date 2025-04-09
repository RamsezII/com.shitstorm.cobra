using _UTIL_;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _COBRA_
{
    public sealed partial class Command
    {
        public readonly Dictionary<string, Command> _commands = new(StringComparer.OrdinalIgnoreCase);
        public IEnumerable<string> ECommands_keys => _commands.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase);
        public IEnumerable<KeyValuePair<string, Command>> ECommands_pairs => _commands.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase);

        public static readonly Command cmd_root_shell = new("root_shell", log_error: true);

        public readonly string name;
        public readonly Traductions manual;
        public readonly bool log_error;
        public readonly int action_min_args_required;
        public readonly int pipe_min_args_required;
        public readonly Action<Executor> args, action;
        public readonly Action<Executor, object> on_pipe;
        public readonly Func<Executor, IEnumerator<CMD_STATUS>> routine;

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            Debug.Log("init shell".ToSubLog());
            cmd_root_shell.PropagateOblivion();
        }

        //--------------------------------------------------------------------------------------------------------------

        public Command(
            in string name,
            in Traductions manual = default,
            in bool log_error = default,
            in int action_min_args_required = default,
            in int pipe_min_args_required = default,
            in Action<Executor> args = default,
            in Action<Executor> action = default,
            in Action<Executor, object> on_pipe = default,
            in Func<Executor, IEnumerator<CMD_STATUS>> routine = default
            )
        {
            this.name = name;
            this.manual = manual;
            this.log_error = log_error;
            this.action_min_args_required = action_min_args_required;
            this.pipe_min_args_required = pipe_min_args_required;
            this.args = args;
            this.action = action;
            this.on_pipe = on_pipe;
            this.routine = routine;
        }

        //--------------------------------------------------------------------------------------------------------------

        public Command AddCommand(in Command command, params string[] aliases)
        {
            _commands.Add(command.name, command);
            for (int i = 0; i < aliases.Length; ++i)
                _commands.Add(aliases[i], command);
            return command;
        }

        public bool TryReadCommand_path(in Line line, out List<KeyValuePair<string, Command>> path)
        {
            path = new();
            line.LintToThisPosition(Color.white);

            bool res = TryReadCommand_ref(line, this, path);

            if (res)
                line.LintToThisPosition(line.linter.command);
            else
                line.LintToThisPosition(line.linter.error);

            return res;

            static bool TryReadCommand_ref(in Line line, in Command parent, in List<KeyValuePair<string, Command>> path)
            {
                if (line.TryReadArgument(out string cmd_name, parent.ECommands_keys, lint: false))
                    if (parent._commands.TryGetValue(cmd_name, out Command intermediate))
                    {
                        path.Add(new(cmd_name, intermediate));
                        TryReadCommand_ref(line, intermediate, path);
                    }
                    else
                        line.ReadBack();
                return path.Count > 0;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        public void PropagateOblivion()
        {
            foreach (Command command in _commands.Values)
                command.PropagateOblivion();
            _commands.Clear();
        }
    }
}