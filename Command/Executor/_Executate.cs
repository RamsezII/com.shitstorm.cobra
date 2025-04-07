using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Executor
        {
            public bool CanLogError()
            {
                if (command != null && line != null)
                    switch (line.signal)
                    {
                        case CMD_SIGNALS.CHECK:
                        case CMD_SIGNALS.EXEC:
                            return command.log_error;
                    }
                return false;
            }

            public bool TryKill()
            {
                if (routine != null)
                    if (cant_be_killed)
                        Debug.LogWarning($"'{cmd_name}' ({cmd_path}) is immortal");
                    else
                    {
                        routine.Dispose();
                        routine = null;
                        return true;
                    }
                return false;
            }

            public IEnumerator<CMD_STATUS> Executate(in Line line)
            {
                this.line = line;

                if (line.signal == CMD_SIGNALS.EXEC)
                    ++executions;

                if (line.notEmpty)
                    if (line.signal >= CMD_SIGNALS.TAB || executions == 0 || routine == null)
                        if (command._commands.Count > 0)
                        {
                            if (command.TryReadCommand_path(line, out var path))
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
                                try
                                {
                                    command.action(this);
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogException(e);
                                    error = $"[{nameof(command.action)}] '{cmd_name}' ({cmd_path}) failed to execute: \"{e.TrimMessage()}\"";
                                }

                        if (error == null)
                            try
                            {
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
                            catch (System.Exception e)
                            {
                                Debug.LogException(e);
                                error = $"[{nameof(command.routine)}] '{cmd_name}' ({cmd_path}) failed to execute: \"{e.TrimMessage()}\"";
                            }
                    }

                if (error == null)
                    return routine;
                else
                {
                    if (CanLogError())
                        Debug.LogWarning($"[ERROR] '{cmd_name}': {error}");
                    return null;
                }
            }
        }
    }
}