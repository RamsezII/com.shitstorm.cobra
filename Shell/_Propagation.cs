using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Shell
    {
        public string PropagateLine(in Command.Line line)
        {
            string error = null;

            if (line.signal.HasFlag(SIGNAL_FLAGS.KILL))
            {
                Debug.LogWarning($"'{GetType().FullName}[{id}] {nameof(line.signal)}: '{line.signal}' to be repaired");
                error = $"[SHELL_WARNING] {nameof(SIGNAL_FLAGS.KILL)} signal received";
            }

            if (line.signal.HasFlag(SIGNAL_FLAGS.TICK))
                if (background_executors.Count > 0)
                    for (int i = 0; i < background_executors.Count; ++i)
                    {
                        var executor = background_executors[i];
                        var routine = executor.routine;

                        if (routine == null || !routine.MoveNext())
                        {
                            executor.Dispose();
                            background_executors.RemoveAt(i);
                            --i;
                        }
                    }

                before_active_executors:

            if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                if (active_executors_stack.Count > 0)
                {
                    var exe = active_executors_stack[^1];
                    if (exe.routine == null)
                    {
                        active_executors_stack.RemoveAt(active_executors_stack.Count - 1);
                        exe.line = line;
                        exe.command.action(exe);
                        exe.line = null;
                        //exe.Dispose();
                        goto before_active_executors;
                    }
                    else
                    {
                        var routine = exe.routine;
                        switch (routine.Current.state)
                        {
                            case CMD_STATES.BLOCKING or CMD_STATES.FULLSCREEN_readonly when line.signal.HasFlag(SIGNAL_FLAGS.TICK):
                                if (!routine.MoveNext())
                                {
                                    exe.Dispose();
                                    active_executors_stack.RemoveAt(active_executors_stack.Count - 1);
                                }
                                break;

                            case CMD_STATES.WAIT_FOR_STDIN:
                            case CMD_STATES.FULLSCREEN_write:
                                exe.line = line;
                                if (!routine.MoveNext())
                                {
                                    exe.Dispose();
                                    active_executors_stack.RemoveAt(active_executors_stack.Count - 1);
                                }
                                exe.line = null;
                                break;
                        }
                    }
                }

            before_pending_queue:

            if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                if (active_executors_stack.Count == 0 && pending_executors_queue.Count > 0)
                {
                    var exe = pending_executors_queue.Dequeue();
                    if (exe == null)
                        error = $"[SHELL_WARNING] presence of NULL {exe.GetType().FullName} in {nameof(pending_executors_queue)}";
                    else if (exe.disposed.Value)
                        error = $"[SHELL_WARNING] oblivion of disposed {exe.GetType().FullName} in {nameof(pending_executors_queue)}";
                    else
                    {
                        active_executors_stack.Add(exe);
                        goto before_active_executors;
                    }
                }

            if (error == null && line.HasNext(true))
            {
                Queue<Command.Executor> add_to_chain = new();
                List<Command.Executor> add_to_background = new();

            before_loop:

                if (error == null && line.HasNext(true))
                    if (!static_domain.TryReadCommand_path(line, out var path))
                    {
                        if (!string.IsNullOrWhiteSpace(line.arg_last))
                            error = $"[USER_ERROR] command '{line.arg_last}' not found in '{static_domain.name}'";
                    }
                    else
                    {
                        Command.Executor exe = new(this, line, path);
                        if (exe.error != null)
                        {
                            error = exe.error;
                            exe.Dispose();
                        }
                        else
                        {
                        before_separator:
                            if (line.TryReadChainSeparator(out string separator))
                                switch (separator)
                                {
                                    case Util_cobra.str_CHAIN:
                                        add_to_chain.Enqueue(exe);
                                        goto before_loop;

                                    case Util_cobra.str_PIPE:
                                        error = $"'{exe.command.name}' ({exe.cmd_path}) {nameof(exe.command.on_pipe)}: {exe.command.on_pipe} is null";
                                        break;

                                    case Util_cobra.str_BACKGROUND:
                                        add_to_background.Add(exe);
                                        Debug.Log($"{exe.id} {nameof(exe.background)}: {exe.id}".ToLog());
                                        exe.Stdout(exe.id);
                                        exe.PropagateBackground();

                                        if (line.HasNext(true))
                                            goto before_separator;
                                        else
                                            break;

                                    default:
                                        if (line.TryReadAny(out string any))
                                            error = $"'{any}' is no valid command separator";
                                        break;
                                }
                            else
                                add_to_chain.Enqueue(exe);

                            if (error != null || !line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                                exe.Dispose();
                        }
                    }

                if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                {
                    if (add_to_background.Count > 0)
                        background_executors.AddRange(add_to_background);

                    if (add_to_chain.Count > 0)
                    {
                        foreach (var exe in add_to_chain)
                            pending_executors_queue.Enqueue(exe);
                        goto before_pending_queue;
                    }
                }
            }
            else
                line.ReadBack();

            if (active_executors_stack.Count == 0)
                status = new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, prefixe: Command.Executor.GetPrefixe(), immortal: true);
            else
                status = active_executors_stack[^1].routine.Current;

            if (error != null)
                if (line.signal.HasFlag(SIGNAL_FLAGS.CHECK))
                    Debug.LogWarning($"[WARN]{GetType().FullName}[{id}] -> {error}");
                else if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                    Debug.LogError($"[ERROR]{GetType().FullName}[{id}] -> {error}");

            return error;
        }
    }
}