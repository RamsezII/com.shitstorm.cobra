using System.Collections.Generic;
using System;
using System.Linq;

namespace _COBRA_
{
    partial class Command
    {
        internal readonly Dictionary<string, Command> _commands = new(StringComparer.OrdinalIgnoreCase);
        internal IEnumerable<string> ECommands_keys => _commands.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase);
        internal IEnumerable<KeyValuePair<string, Command>> ECommands_pairs => _commands.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase);

        //--------------------------------------------------------------------------------------------------------------

        public bool TryRemoveCommand(in string cmd_name)
        {
            if (_commands.TryGetValue(cmd_name, out Command command))
                return TryRemoveCommand(command);
            return false;
        }

        public bool TryRemoveCommand(in Command command)
        {
            HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in _commands)
                if (pair.Value == command)
                    keys.Add(pair.Key);

            if (keys.Count == 0)
                return false;

            foreach (var key in keys)
                _commands.Remove(key);
            return true;
        }

        public bool TryReadCommand(in Signal signal, out Command command)
        {
            if (TryReadCommand_path(signal, out var path))
            {
                command = path[^1];
                return true;
            }
            command = default;
            return false;
        }

        public bool TryReadCommand_path(in Signal signal, out List<Command> path, in bool pipe_only = false)
        {
            path = new();
            signal.LintToThisPosition(signal.linter._default_);

            bool res = TryReadCommand_ref(signal, this, pipe_only, path);
            if (res)
                signal.LintToThisPosition(signal.linter.command);
            else
            {
                path = null;
                signal.LintToThisPosition(signal.linter.error);
            }
            return res && path != null && path.Count > 0;

            static bool TryReadCommand_ref(in Signal signal, Command domain, bool pipe_only, in List<Command> path)
            {
                IEnumerable<string> keys = domain.ECommands_keys;
                if (pipe_only)
                    keys = keys.Where(keys => domain._commands[keys].on_pipe != null);

                if (signal.TryReadArgument(out string cmd_name, out bool is_candidate, keys, strict: true, stop_if_var: true, lint: false))
                    if (is_candidate && domain._commands.TryGetValue(cmd_name, out Command intermediate))
                    {
                        path.Add(intermediate);
                        if (intermediate.IsDomain)
                            return TryReadCommand_ref(signal, intermediate, pipe_only, path);
                    }

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