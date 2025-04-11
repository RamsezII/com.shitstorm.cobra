using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Shell
    {
        internal void SpontaneizeExecutor(in Command.Executor executor)
        {
            if (active_exe_pipelines_stack.Count == 0)
            {
                ExecutorPipeline pipeline = new();
                pipeline.executors.Add(executor);
                active_exe_pipelines_stack.Add(pipeline);
            }
            else
                active_exe_pipelines_stack[^1].executors.Add(executor);
        }

        void TickExecutors() => PropagateLine(new Command.Line(string.Empty, SIGNAL_FLAGS.TICK, terminal));
        public string PropagateLine(in Command.Line line)
        {
            string error = null;

            if (line.signal.HasFlag(SIGNAL_FLAGS.KILL))
            {
                Debug.LogWarning($"'{GetType().FullName}[{id}] {nameof(line.signal)}: '{line.signal}' to be repaired");
                error = $"[SHELL_WARNING] {nameof(SIGNAL_FLAGS.KILL)} signal received";
            }

            if (background_executors.Count > 0)
                if (line.signal.HasFlag(SIGNAL_FLAGS.TICK))
                    for (int i = 0; i < background_executors.Count; ++i)
                    {
                        var exe = background_executors[i];
                        var routine = exe.routine;

                        exe.line = line;
                        if (routine == null || !routine.MoveNext())
                        {
                            exe.Dispose();
                            background_executors.RemoveAt(i);
                            --i;
                        }
                        exe.line = null;
                    }

                before_active_executors:

            if (line.HasFlags_any(SIGNAL_FLAGS.EXEC | SIGNAL_FLAGS.TICK))
                if (active_exe_pipelines_stack.Count > 0)
                {
                    ExecutorPipeline exe_pipeline = active_exe_pipelines_stack[^1];
                    if (!exe_pipeline.TryGetCurrent(out Command.Executor exe))
                    {
                        active_exe_pipelines_stack.RemoveAt(active_exe_pipelines_stack.Count - 1);
                        goto before_active_executors;
                    }
                    else
                    {
                        exe.line = line;

                        if (exe.routine == null)
                        {
                            if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                                exe.command.action(exe);
                            exe.Dispose();
                        }
                        else if (line.signal.HasFlag(SIGNAL_FLAGS.TICK) && !exe.routine.MoveNext())
                            exe.Dispose();

                        exe.line = null;

                        if (exe.disposed.Value)
                        {
                            if (exe_pipeline.AreAllDisposed())
                            {
                                active_exe_pipelines_stack.RemoveAt(active_exe_pipelines_stack.Count - 1);
                                exe_pipeline.Dispose();
                            }

                            goto before_active_executors;
                        }
                    }
                }

            before_pending_queue:

            if (line.HasFlags_any(SIGNAL_FLAGS.EXEC | SIGNAL_FLAGS.TICK))
                if (active_exe_pipelines_stack.Count == 0 && pending_executors_queue.Count > 0)
                {
                    var exe = pending_executors_queue.Dequeue();
                    if (exe == null)
                        error = $"[SHELL_WARNING] presence of NULL {exe.GetType().FullName} in {nameof(pending_executors_queue)}";
                    else if (exe.disposed.Value)
                        error = $"[SHELL_WARNING] oblivion of disposed {exe.GetType().FullName} in {nameof(pending_executors_queue)}";
                    else
                    {
                        SpontaneizeExecutor(exe);
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
                            if (line.TryReadCommandSeparator(out string separator))
                                switch (separator)
                                {
                                    case Util_cobra.str_CHAIN:
                                        add_to_chain.Enqueue(exe);
                                        goto before_loop;

                                    case Util_cobra.str_PIPE:
                                        error = $"'{exe.command.name}' ({exe.cmd_path}) {nameof(exe.command.on_pipe)}: {exe.command.on_pipe} is null";
                                        break;

                                    case Util_cobra.str_BACKGROUND:
                                        if (exe.command.routine == null)
                                            error = $"'{exe.command.name}' ({exe.cmd_path}) can not run in background because it has no {nameof(exe.command.routine)}";
                                        else
                                        {
                                            add_to_background.Add(exe);

                                            if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                                            {
                                                Debug.Log($"[{exe.id}] '{exe.command.name}' ({exe.cmd_path}) started running in background".ToSubLog());
                                                exe.Stdout(exe.id);
                                            }

                                            if (line.HasNext(true))
                                                goto before_separator;
                                            else
                                                break;
                                        }
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

            if (active_exe_pipelines_stack.Count > 0 && active_exe_pipelines_stack[^1].TryGetCurrent(out Command.Executor active_exe))
                status = active_exe.routine.Current;
            else
                status = new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, prefixe: Command.Executor.GetPrefixe(), immortal: true);

            if (error != null)
                if (line.signal.HasFlag(SIGNAL_FLAGS.CHECK))
                    Debug.LogWarning($"[WARN]{GetType().FullName}[{id}] -> {error}");
                else if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                    Debug.LogError($"[ERROR]{GetType().FullName}[{id}] -> {error}");

            return error;
        }
    }
}