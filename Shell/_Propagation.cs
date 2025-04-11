using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Shell
    {
        void TickExecutors() => PropagateLine(new Command.Line(string.Empty, SIGNAL_FLAGS.TICK, terminal));
        public string PropagateLine(in Command.Line line)
        {
            string error = null;

            if (line.signal.HasFlag(SIGNAL_FLAGS.KILL))
            {
                Debug.LogWarning($"'{GetType().FullName}[{id}] {nameof(line.signal)}: '{line.signal}' to be repaired");
                error = $"[SHELL_WARNING] {nameof(SIGNAL_FLAGS.KILL)} signal received";
            }

            if (line.signal.HasFlag(SIGNAL_FLAGS.TICK))
                if (background_executors_pipelines.Count > 0)
                    for (int i = 0; i < background_executors_pipelines.Count; ++i)
                    {
                        ExecutorPipeline pipeline = background_executors_pipelines[i];
                        if (!pipeline.TryExecuteCurrent(line, out Command.Executor exe))
                            background_executors_pipelines.RemoveAt(i--);
                    }

                before_active_executors:

            if (line.HasFlags_any(SIGNAL_FLAGS.EXEC | SIGNAL_FLAGS.TICK))
                if (active_executor_pipelines_stack.Count > 0)
                {
                    ExecutorPipeline exe_pipeline = active_executor_pipelines_stack[^1];
                    if (exe_pipeline.TryExecuteCurrent(line, out Command.Executor exe))
                    {
                        if (exe.disposed.Value)
                        {
                            if (exe_pipeline.AreAllDisposed())
                            {
                                active_executor_pipelines_stack.RemoveAt(active_executor_pipelines_stack.Count - 1);
                                exe_pipeline.Dispose();
                            }
                            goto before_active_executors;
                        }
                    }
                    else
                    {
                        active_executor_pipelines_stack.RemoveAt(active_executor_pipelines_stack.Count - 1);
                        exe_pipeline.Dispose();
                        goto before_active_executors;
                    }
                }

            before_pending_queue:

            if (line.HasFlags_any(SIGNAL_FLAGS.EXEC | SIGNAL_FLAGS.TICK))
                if (active_executor_pipelines_stack.Count == 0 && pending_executors_queue.Count > 0)
                {
                    var exe = pending_executors_queue.Dequeue();
                    if (exe == null)
                        error = $"[SHELL_WARNING] presence of NULL {exe.GetType().FullName} in {nameof(pending_executors_queue)}";
                    else if (exe.disposed.Value)
                        error = $"[SHELL_WARNING] oblivion of disposed {exe.GetType().FullName} in {nameof(pending_executors_queue)}";
                    else
                    {
                        if (exe.background)
                        {
                            exe.LogBackgroundStart();
                            background_executors_pipelines.Add(new ExecutorPipeline(exe));
                        }
                        else
                            active_executor_pipelines_stack.Add(new ExecutorPipeline(exe));
                        goto before_active_executors;
                    }
                }

            if (error == null && line.HasNext(true))
            {
                List<Command.Executor> add_to_chain = null;
                if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                    add_to_chain = new();

                before_loop:

                if (error == null && line.HasNext(true))
                    if (!static_domain.TryReadCommand_path(line, out var path))
                    {
                        if (!string.IsNullOrWhiteSpace(line.arg_last))
                            error = $"'{line.arg_last}' not found in '{static_domain.name}'";
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
                                        if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                                            add_to_chain.Add(exe);
                                        goto before_loop;

                                    case Util_cobra.str_PIPE:
                                        error = $"'{exe.command.name}' ({exe.cmd_path}) {nameof(exe.command.on_pipe)}: {exe.command.on_pipe} is null";
                                        break;

                                    case Util_cobra.str_BACKGROUND:
                                        exe.PropagateBackground();
                                        if (line.HasNext(true))
                                            goto before_separator;
                                        else
                                        {
                                            add_to_chain.Add(exe);
                                            break;
                                        }

                                    default:
                                        if (line.TryReadAny(out string any))
                                            error = $"{nameof(PropagateLine).Italic()} '{any}' is no valid command separator";
                                        break;
                                }
                            else if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                                add_to_chain.Add(exe);

                            if (error != null || !line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                                exe.Dispose();
                        }
                    }

                if (error == null)
                    if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                        if (add_to_chain.Count > 0)
                        {
                            for (int i = 0; i < add_to_chain.Count; i++)
                            {
                                Command.Executor exe = add_to_chain[i];
                                if (!exe.background)
                                    pending_executors_queue.Enqueue(exe);
                                else
                                {
                                    exe.LogBackgroundStart();
                                    exe.PropagateBackground();
                                    background_executors_pipelines.Add(new ExecutorPipeline(exe));
                                }
                            }

                            add_to_chain.Clear();
                            goto before_pending_queue;
                        }
            }
            else
                line.ReadBack();

            if (active_executor_pipelines_stack.Count > 0 && active_executor_pipelines_stack[^1].TryGetCurrent(out Command.Executor active_exe))
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