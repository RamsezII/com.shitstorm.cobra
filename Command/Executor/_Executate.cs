using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Executor
        {
            public IEnumerator<CMD_STATUS> Executate(in Line line)
            {
                this.line = line;

                if (line.signal == CMD_SIGNALS.EXEC)
                    ++executions;

                if (line.notEmpty)
                    if (line.signal >= CMD_SIGNALS.TAB || executions == 0 || routine == null)
                        if (command._commands.Count > 0)
                        {
                            if (command.TryReadCommand(line, out var path))
                            {
                                Executor exe = new(path, line);
                                error = exe.error;
                                if (exe.error == null)
                                    return routine = exe.Executate(line);
                                else
                                    exe.Dispose();
                            }
                            else if (line.signal == CMD_SIGNALS.EXEC)
                                error = $"Could not find '{line.arg_last}' in '{cmd_name}' ({cmd_path})";
                        }

                if (error == null)
                    if (line.signal == CMD_SIGNALS.EXEC)
                    {
                        if (command.action != null)
                            if (command.action_min_args_required > 0 && args != null && args.Count < command.action_min_args_required)
                                error = $"[{nameof(command.action_min_args_required)}] '{cmd_name}' ({cmd_path}) requires {command.action_min_args_required} arguments to execute, {args.Count} were given.";
                            else
                                command.action(this);

                        if (error == null)
                            if (executions == 0)
                            {
                                if (command.routine != null)
                                {
                                    routine = command.routine(this);
                                    routine.MoveNext();
                                    return routine;
                                }
                            }
                            else if (routine != null && !routine.MoveNext())
                                routine = null;
                    }

                if (error == null)
                    return routine;
                else
                {
                    Debug.LogWarning($"[ERROR] '{cmd_name}': {error}");
                    return null;
                }
            }
        }
    }
}