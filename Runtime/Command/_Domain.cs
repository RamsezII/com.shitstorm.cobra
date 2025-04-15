using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

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

        public bool TryReadCommand(in Line line, out Command command)
        {
            if (TryReadCommand_path(line, out var path))
            {
                command = path[^1];
                return true;
            }
            command = default;
            return false;
        }

        public bool TryReadCommand_path(in Line line, out List<Command> path, in bool pipe_only = false)
        {
            path = new();
            line.LintToThisPosition(Color.white);

            bool res = TryReadCommand_ref(line, this, pipe_only, path);
            if (res)
                line.LintToThisPosition(line.linter.command);
            else
            {
                path = null;
                line.LintToThisPosition(line.linter.error);
            }
            return res && path != null && path.Count > 0;

            static bool TryReadCommand_ref(in Line line, Command domain, bool pipe_only, in List<Command> path)
            {
                IEnumerable<string> keys = domain.ECommands_keys;
                if (pipe_only)
                    keys = keys.Where(keys => domain._commands[keys].on_pipe != null);

                if (line.TryReadArgument(out string cmd_name, out bool is_candidate, keys, lint: false) && (!pipe_only || is_candidate) && domain._commands.TryGetValue(cmd_name, out Command intermediate))
                {
                    path.Add(intermediate);
                    if (intermediate.IsDomain)
                        return TryReadCommand_ref(line, intermediate, pipe_only, path);
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