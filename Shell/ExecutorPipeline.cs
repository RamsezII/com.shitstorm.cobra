using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    internal class ExecutorPipeline : IDisposable
    {
        readonly List<Command.Executor> _executors = new();
        bool disposed;

        //--------------------------------------------------------------------------------------------------------------

        internal ExecutorPipeline(in Command.Executor executor) => AddExecutor(executor);

        //--------------------------------------------------------------------------------------------------------------

        internal void AddExecutor(in Command.Executor executor)
        {
            if (disposed)
                Debug.LogError($"adding {executor.GetType().FullName} '{executor.command.name}' ({executor.cmd_path}) to a disposed {GetType().FullName}.");
            executor.pipeline = this;
            if (!_executors.Remove(executor) && executor.background)
                executor.LogBackgroundStart();
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
                    return true;
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
                    exe.command.action(exe);
                    exe.Dispose();
                }

            if (exe.routine != null)
                if (line.signal.HasFlag(SIGNAL_FLAGS.TICK))
                    if (!exe.routine.MoveNext())
                        exe.Dispose();

            exe.line = null;

            if (exe.disposed)
                goto before_execution;

            return true;
        }

        //--------------------------------------------------------------------------------------------------------------

        internal bool AreAllDisposed()
        {
            for (int i = 0; i < _executors.Count; i++)
                if (!_executors[i].disposed)
                    return false;
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