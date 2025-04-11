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
                Debug.LogWarning($"'{GetType().FullName}[{shell_ID}] {nameof(line.signal)}: '{line.signal}' to be repaired");
                error = $"[SHELL_WARNING] {nameof(SIGNAL_FLAGS.KILL)} signal received";
            }

            if (line.signal.HasFlag(SIGNAL_FLAGS.TICK))
                if (background_janitors.Count > 0)
                    for (int i = 0; i < background_janitors.Count; ++i)
                    {
                        Command.Executor.Janitor janitor = background_janitors[i];
                        if (!janitor.TryExecuteCurrent(line, background_janitors, out _))
                            background_janitors.RemoveAt(i--);
                    }

                before_active_executors:

            if (line.HasFlags_any(SIGNAL_FLAGS.EXEC | SIGNAL_FLAGS.TICK))
                if (front_janitors.Count > 0)
                {
                    Command.Executor.Janitor janitor = front_janitors[^1];
                    if (!janitor.TryExecuteCurrent(line, background_janitors, out _))
                    {
                        front_janitors.RemoveAt(front_janitors.Count - 1);
                        janitor.Dispose();
                        goto before_active_executors;
                    }
                }

            before_pending_queue:

            if (line.HasFlags_any(SIGNAL_FLAGS.EXEC | SIGNAL_FLAGS.TICK))
                if (front_janitors.Count == 0 && pending_executors.Count > 0)
                {
                    var exe = pending_executors.Dequeue();
                    if (exe == null)
                        error = $"[SHELL_WARNING] presence of NULL {exe.GetType().FullName} in {nameof(pending_executors)}";
                    else if (exe.disposed)
                        error = $"[SHELL_WARNING] oblivion of disposed {exe.GetType().FullName} ({exe}) in {nameof(pending_executors)}";
                    else if (exe.background)
                        background_janitors.Add(new Command.Executor.Janitor(exe));
                    else
                    {
                        front_janitors.Add(new Command.Executor.Janitor(exe));
                        goto before_active_executors;
                    }
                }

            if (error == null && line.HasNext(true))
                if (static_domain.TryReadCommand_path(line, out var path))
                {
                    Command.Executor exe = new(this, null, line, path);
                    if (exe.error != null)
                    {
                        error = exe.error;
                        exe.Dispose();
                    }
                    else if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                        if (exe.background)
                        {
                            exe.PropagateBackground();
                            background_janitors.Add(new Command.Executor.Janitor(exe));
                        }
                        else
                        {
                            pending_executors.Enqueue(exe);
                            goto before_pending_queue;
                        }
                }
                else if (!string.IsNullOrWhiteSpace(line.arg_last))
                    error = $"'{line.arg_last}' not found in '{static_domain.name}'";

            previous_state = status.state;
            if (front_janitors.Count > 0 && front_janitors[^1].TryGetCurrent(out Command.Executor active_exe))
                status = active_exe.routine.Current;
            else
                status = new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, prefixe: Command.Executor.GetPrefixe(), immortal: true);
            state_changed = previous_state != status.state;

            if (error != null)
                if (line.signal.HasFlag(SIGNAL_FLAGS.CHECK))
                    Debug.LogWarning($"[WARN {this}] -> {error}");
                else if (line.signal.HasFlag(SIGNAL_FLAGS.EXEC))
                    Debug.LogError($"[ERROR {this}] -> {error}");

            return error;
        }
    }
}