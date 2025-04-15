using UnityEngine;

namespace _COBRA_
{
    partial class Shell
    {
        static void UpdateBackgroundJanitors()
        {
            Command.Line line = new(string.Empty, SIGNALS.TICK, null);
            if (background_janitors.Count > 0)
                for (int i = 0; i < background_janitors.Count; ++i)
                {
                    var janitor = background_janitors[i];
                    bool in_activity = janitor.TryExecuteCurrent(line, out _);

                    if (janitor.TryPullError(out string error))
                        Debug.LogError($"[BACKGROUND_ERROR] janitor[{i}] {error}");

                    if (!in_activity || error != null)
                    {
                        janitor.Dispose();
                        background_janitors.RemoveAt(i--);
                    }
                }
        }

        void UpdateUpdateJanitors() => PropagateLine(new Command.Line(string.Empty, SIGNALS.TICK, this));
        public string PropagateLine(in Command.Line line)
        {
            string error = null;

            if (error == null)
                if (line.signal.HasFlag(SIGNALS.KILL))
                    if (front_janitors.Count > 0)
                    {
                        var janitor = front_janitors[^1];
                        if (janitor.TryGetCurrent(out var exe))
                            if (exe.routine != null)
                                if (exe.command.immortal)
                                {
                                    error = $"{line.signal} signal received on an immortal executor: {exe}";
                                    line.data = new CMDLINE_DATA(CMDLINE_STATUS.REJECT, error);
                                }
                                else
                                {
                                    exe.line = line;
                                    exe.routine.MoveNext();
                                    line.data = new CMDLINE_DATA(CMDLINE_STATUS.CONFIRM, $"{line.signal} killed current executor: {exe}");
                                    exe.line = null;
                                    exe.Dispose();
                                }
                    }

                before_active_executors:

            if (error == null)
                if (front_janitors.Count > 0)
                {
                    var janitor = front_janitors[^1];
                    bool in_activity = janitor.TryExecuteCurrent(line, out _);

                    janitor.TryPullError(out error);

                    if (!in_activity || error != null)
                    {
                        janitor.Dispose();
                        front_janitors.Remove(janitor);

                        if (error == null)
                            goto before_active_executors;
                    }
                }

            before_pending_queue:

            if (error == null)
                if (line.HasFlags_any(SIGNALS.EXEC | SIGNALS.TICK))
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

            if (front_janitors.Count == 0)
                if (error == null && line.HasNext(true))
                    if (Command.static_domain.TryReadCommand_path(line, out var path))
                    {
                        Command.Executor exe = new(this, null, line, path);
                        if (exe.error != null)
                        {
                            error = exe.error;
                            exe.Dispose();
                        }
                        else if (line.signal.HasFlag(SIGNALS.EXEC))
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
                        error = $"'{line.arg_last}' not found in '{Command.static_domain.name}'";

            previous_state = current_status.state;
            if (front_janitors.Count > 0 && front_janitors[^1].TryGetCurrent(out Command.Executor active_exe))
                current_status = active_exe.routine.Current;
            else
                current_status = new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, prefixe: GetPrefixe(), immortal: true);
            state_changed = previous_state != current_status.state;

            if (error != null)
                if (line.signal.HasFlag(SIGNALS.CHECK))
                    Debug.LogWarning($"[WARN {this}] -> {error}");
                else if (line.signal.HasFlag(SIGNALS.EXEC))
                    Debug.LogError($"[ERROR {this}] -> {error}");

            return error;
        }
    }
}