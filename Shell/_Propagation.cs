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
                        if (!pipeline.TryExecuteCurrent(line, out _))
                            background_executors_pipelines.RemoveAt(i--);
                    }

                before_active_executors:

            if (line.HasFlags_any(SIGNAL_FLAGS.EXEC | SIGNAL_FLAGS.TICK))
                if (active_executor_pipelines_stack.Count > 0)
                {
                    ExecutorPipeline pipeline = active_executor_pipelines_stack[^1];
                    if (!pipeline.TryExecuteCurrent(line, out _))
                    {
                        active_executor_pipelines_stack.RemoveAt(active_executor_pipelines_stack.Count - 1);
                        pipeline.Dispose();
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
                    else if (exe.disposed)
                        error = $"[SHELL_WARNING] oblivion of disposed {exe.GetType().FullName} in {nameof(pending_executors_queue)}";
                    else if (exe.background)
                        background_executors_pipelines.Add(new ExecutorPipeline(exe));
                    else
                    {
                        active_executor_pipelines_stack.Add(new ExecutorPipeline(exe));
                        goto before_active_executors;
                    }
                }

            if (error == null && line.HasNext(true))
                if (static_domain.TryReadCommand_path(line, out var path))
                {
                    Command.Executor exe = new(this, line, path);
                    if (exe.error != null)
                    {
                        error = exe.error;
                        exe.Dispose();
                    }
                    else if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                        if (exe.background)
                        {
                            exe.LogBackgroundStart();
                            exe.PropagateBackground();
                            background_executors_pipelines.Add(new ExecutorPipeline(exe));
                        }
                        else
                        {
                            pending_executors_queue.Enqueue(exe);
                            goto before_pending_queue;
                        }
                }
                else if (!string.IsNullOrWhiteSpace(line.arg_last))
                    error = $"'{line.arg_last}' not found in '{static_domain.name}'";

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