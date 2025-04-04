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

                if (line.signal >= CMD_SIGNALS.TAB || executions == 0 || routine == null)
                    if (command._commands.Count > 0)
                    {
                        if (command.TryReadCommand(line, out var path))
                        {
                            Executor exe = new(path, line);
                            if (exe.error == null)
                                return routine = exe.Executate(line);
                        }
                        else if (line.signal == CMD_SIGNALS.EXEC)
                            Debug.LogWarning($"Could not find '{line.arg_last}' in '{cmd_name}' ({cmd_path})");
                        return null;
                    }

                if (line.signal == CMD_SIGNALS.EXEC)
                {
                    Line.AddToHistory(line.text);
                    command.action?.Invoke(this);

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

                return routine;
            }
        }
    }
}