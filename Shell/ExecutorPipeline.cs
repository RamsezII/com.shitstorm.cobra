using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    internal class ExecutorPipeline : IDisposable
    {
        readonly List<Command.Executor> _executors = new();
        bool disposed;

        static ushort id_counter;
        public readonly ushort pipeline_ID = ++id_counter;

        //--------------------------------------------------------------------------------------------------------------

        internal ExecutorPipeline(in Command.Executor executor) => AddExecutor(executor);

        //--------------------------------------------------------------------------------------------------------------

        internal void AddExecutor(in Command.Executor executor)
        {
            if (disposed)
                Debug.LogError($"adding {executor.GetType().FullName} '{executor.command.name}' ({executor.cmd_path}) to disposed pipeline[{pipeline_ID}].");

            executor.pipeline = this;

            if (_executors.Remove(executor))
                Debug.LogWarning($"'{executor.GetType().FullName}' '{executor.command.name}' ({executor.cmd_path}) already exists in pipeline[{pipeline_ID}]. Replacing it.");

            _executors.Add(executor);
        }

        internal bool TryGetCurrent(out Command.Executor executor)
        {
            for (int i = 0; i < _executors.Count; i++)
            {
                executor = _executors[i];
                if (!executor.disposed)
                    return true;
                else if (executor.TryPullNext(out executor))
                {
                    _executors.Add(executor);
                    return true;
                }
            }
            executor = null;
            return false;
        }

        internal bool TryExecuteCurrent(in Command.Line line, out Command.Executor exe)
        {
        before_execution:
            if (!TryGetCurrent(out exe))
            {
                Dispose();
                return false;
            }

            if (exe.command.action == null && exe.routine == null)
            {
                Debug.LogError($"'{exe.GetType().FullName}' '{exe.command.name}' ({exe.cmd_path}) has no {nameof(exe.command.action)} or {nameof(exe.routine)} to execute.");
                return false;
            }

            exe.line = line;

            if (exe.command.action != null)
                if (line.HasFlags_any(SIGNAL_FLAGS.EXEC | SIGNAL_FLAGS.TICK))
                {
                    if (exe.started)
                        Debug.LogWarning($"'{exe.GetType().FullName}' '{exe.command.name}' ({exe.cmd_path}) has already been started.");

                    exe.started = true;
                    if (exe.background)
                        exe.LogBackgroundStart();

                    exe.command.action(exe);
                    exe.Dispose();
                }

            if (exe.routine != null)
                if (line.signal.HasFlag(SIGNAL_FLAGS.TICK))
                {
                    if (!exe.started && exe.background)
                        exe.LogBackgroundStart();
                    exe.started = true;

                    if (!exe.routine.MoveNext())
                        exe.Dispose();
                }

            exe.line = null;

            if (exe.disposed)
                goto before_execution;

            return true;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            disposed = true;
            for (int i = 0; i < _executors.Count; i++)
                _executors[i].PropagateDispose();
            _executors.Clear();
        }
    }
}