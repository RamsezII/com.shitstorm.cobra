using UnityEngine;

namespace _COBRA_
{
    partial class Shell
    {
        int last_tick;

        //--------------------------------------------------------------------------------------------------------------

        static void UpdateBackgroundJanitors()
        {
            Command.Line line = new(string.Empty, SIG_FLAGS.TICK, null);
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

        void UpdateUpdateJanitors() => PropagateSignal(new Command.Line(string.Empty, SIG_FLAGS.TICK, this));
        public string PropagateSignal(in Command.Line line)
        {
            string error = null;

            if (line.flags.HasFlag(SIG_FLAGS.TICK))
            {
                if (Time.frameCount == last_tick)
                    error = $"tick redundancy ({nameof(Time.frameCount)}: {Time.frameCount})";
                last_tick = Time.frameCount;
            }

            // kill top executor
            if (error == null)
                if (line.flags.HasFlag(SIG_FLAGS.KILL))
                    if (front_janitors.Count > 0)
                    {
                        var janitor = front_janitors[^1];
                        if (janitor.TryGetCurrent(out var exe))
                            if (exe.routine != null)
                                if (exe.command.immortal)
                                {
                                    error = $"{line.flags} signal received on an immortal executor: {exe}";
                                    line.data = new CMDLINE_DATA(CMDLINE_STATUS.REJECT, error);
                                }
                                else
                                {
                                    exe.line = line;
                                    exe.routine.MoveNext();
                                    line.data = new CMDLINE_DATA(CMDLINE_STATUS.CONFIRM, $"{line.flags} killed current executor: {exe}");
                                    exe.line = null;
                                    exe.Dispose();
                                }
                    }

                // executate top executor
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

            // pull pending queue
            before_pending_queue:
            if (error == null)
                if (line.HasFlags_any(SIG_FLAGS.EXEC | SIG_FLAGS.TICK))
                    if (front_janitors.Count == 0 && pending_executors.Count > 0)
                    {
                        var exe = pending_executors.Dequeue();
                        if (exe == null)
                            error = $"[SHELL_WARNING] presence of NULL {exe.GetType().FullName} in {nameof(pending_executors)}";
                        else if (exe.disposed)
                            error = $"[SHELL_WARNING] oblivion of disposed {exe.GetType().FullName} ({exe}) in {nameof(pending_executors)}";
                        else if (exe.background)
                            background_janitors.Add(new Command.Executor.Janitor(line, exe));
                        else
                        {
                            front_janitors.Add(new Command.Executor.Janitor(line, exe));
                            goto before_active_executors;
                        }
                    }

            // parse stdin as new command line
            //if (front_janitors.Count == 0)
            if (error == null && line.HasNext(true))
                if (Command.static_domain.TryReadCommand_path(line, out var path))
                {
                    Command.Executor exe = new(this, null, line, path);
                    if (exe.error != null)
                    {
                        error = exe.error;
                        exe.Dispose();
                    }
                    else if (line.flags.HasFlag(SIG_FLAGS.EXEC))
                        if (exe.background)
                        {
                            exe.PropagateBackground();
                            background_janitors.Add(new Command.Executor.Janitor(line, exe));
                        }
                        else
                        {
                            pending_executors.Enqueue(exe);
                            goto before_pending_queue;
                        }
                }
                else if (!string.IsNullOrWhiteSpace(line.arg_last))
                    error = $"'{line.arg_last}' not found in '{Command.static_domain.name}'";

            // update state
            previous_state = current_status.state;
            if (front_janitors.Count > 0 && front_janitors[^1].TryGetCurrent(out Command.Executor active_exe) && active_exe.routine != null)
                current_status = active_exe.routine.Current;
            else
                current_status = new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, prefixe: GetPrefixe(), immortal: true);
            state_changed = previous_state != current_status.state;

            // show error
            if (error != null)
                if (line.flags.HasFlag(SIG_FLAGS.CHECK))
                    Debug.LogWarning($"[WARN {this}] signal[{line.flags}] -> {error}");
                else if (line.flags.HasFlag(SIG_FLAGS.EXEC))
                    Debug.LogError($"[ERROR {this} signal[{line.flags}] -> {error}");

            // null if everything good
            return error;
        }
    }
}