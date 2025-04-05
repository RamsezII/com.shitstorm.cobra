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

        public static readonly Command cmd_root_shell = new(stay_alive: true, log_error: true);

        public readonly Traductions manual;
        public readonly bool stay_alive, log_error;
        public readonly int init_min_args_required;
        public readonly int action_min_args_required;
        public readonly Action<Executor> args, action;
        public readonly Action<Executor, object> on_data;
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
            in Traductions manual = default,
            in bool stay_alive = default,
            in bool log_error = default,
            in int init_min_args_required = default,
            in int action_min_args_required = default,
            in Action<Executor> args = default,
            in Action<Executor> action = default,
            in Action<Executor, object> on_data = default,
            in Func<Executor, IEnumerator<CMD_STATUS>> routine = default
            )
        {
            this.manual = manual;
            this.stay_alive = stay_alive;
            this.log_error = log_error;
            this.init_min_args_required = init_min_args_required;
            this.action_min_args_required = action_min_args_required;
            this.args = args;
            this.action = action;
            this.on_data = on_data;
            this.routine = routine;
        }

        //--------------------------------------------------------------------------------------------------------------

        public Command AddCommand(in string cmd_name, in Command command, params string[] aliases)
        {
            _commands.Add(cmd_name, command);
            for (int i = 0; i < aliases.Length; ++i)
                _commands.Add(aliases[i], command);
            return command;
        }

        public bool TryReadCommand(in Line line, out Command command)
        {
            if (TryReadCommand_path(line, out var path))
            {
                command = path[^1].Value;
                return true;
            }
            command = null;
            return false;
        }

        public bool TryReadCommand_path(in Line line, out List<KeyValuePair<string, Command>> path)
        {
            path = new();
            return TryReadCommand_ref(line, this, path);

            static bool TryReadCommand_ref(in Line line, in Command parent, in List<KeyValuePair<string, Command>> path)
            {
                if (line.TryReadArgument(out string cmd_name, parent.ECommands_keys))
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