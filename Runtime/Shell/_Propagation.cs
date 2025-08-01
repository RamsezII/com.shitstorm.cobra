﻿using _ARK_;
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

        void PropagateTick()
        {
            if (Time.frameCount == last_tick)
                Debug.LogWarning($"avoided {SIG_FLAGS.TICK} redundancy ({nameof(Time.frameCount)}: {Time.frameCount})", this);
            else
                PropagateSignal(new Command.Line(string.Empty, SIG_FLAGS.TICK, this));
        }

        public string PropagateSignal(in Command.Line line)
        {
            string error = null;
            previous_state = current_state;

            if (line.flags.HasFlag(SIG_FLAGS.TICK))
            {
                if (Time.frameCount == last_tick)
                    Debug.LogWarning($"tick redundancy ({nameof(Time.frameCount)}: {Time.frameCount}, {line.flags.GetType()}: {line.flags})", this);
                last_tick = Time.frameCount;
            }

            // kill top executor
            if (error == null)
                if (line.flags.HasFlag(SIG_FLAGS.KILL))
                    if (front_janitors.TryPeek(out var janitor))
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

                            // executate top janitor
                            before_top_janitor:
            if (error == null)
                if (front_janitors.TryPeek(out var janitor))
                {
                    bool in_activity = janitor.TryExecuteCurrent(line, out _);
                    current_state = new(janitor.current_eid, janitor.exe_status);

                    janitor.TryPullError(out error);

                    if (!in_activity || error != null)
                    {
                        janitor.Dispose();
                        front_janitors.Dequeue();

                        if (error == null)
                            goto before_top_janitor;
                    }
                }
                else
                    current_state = new(
                        SID,
                        new CMD_STATUS(
                            CMD_STATES.WAIT_FOR_STDIN,
                            prefixe: GetPrefixe(),
                            immortal: true)
                        );

            stdin_change_flag |= stdin_change = !current_state.Equals(previous_state);

            // parse stdin as new command line
            if (error == null)
                if (line.HasNext(true))
                    if (Command.static_domain.TryReadCommand_path(line, out var path))
                    {
                        Command.Executor exe = new(this, null, line, path);
                        if (exe.error != null)
                        {
                            error = exe.error;
                            exe.Dispose();
                        }
                        else if (line.flags.HasFlag(SIG_FLAGS.SUBMIT))
                        {
                            if (exe.background)
                                exe.PropagateBackground();

                            line.janitor = new Command.Executor.Janitor(line, exe);

                            if (exe.background)
                                background_janitors.Add(line.janitor);
                            else
                                front_janitors.Enqueue(line.janitor);
                        }
                        else if (line.flags.HasFlag(SIG_FLAGS.TICK))
                            error = $"{typeof(SIG_FLAGS)}.{SIG_FLAGS.TICK} not intercepted ('{line.text[line.start_i..]}')";
                    }
                    else if (!string.IsNullOrWhiteSpace(line.arg_last))
                        error = $"'{line.arg_last}' not found in '{Command.static_domain.name}'";

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