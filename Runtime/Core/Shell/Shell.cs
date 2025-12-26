using _ARK_;
using _UTIL_;
using System;

namespace _COBRA_
{
    public abstract partial class Shell : Disposable
    {
        public readonly ValueHandler<ExecutionStatus> status = new();
        public Action<object, string> stdout, stderr;
        public Action beforeTick, afterTick;
        public bool started;
        public int tick_count;

        //----------------------------------------------------------------------------------------------------------

        protected Shell(in string name) : base(name)
        {
            status.Value = RegularStatus();
        }

        //----------------------------------------------------------------------------------------------------------

        public void ToggleTick(in bool toggle)
        {
            NUCLEOR.delegates.Update_OnShellTick -= Tick;
            if (toggle)
                NUCLEOR.delegates.Update_OnShellTick += Tick;
        }

        //----------------------------------------------------------------------------------------------------------

        public abstract void OnReader(in CodeReader reader);

        public void Tick()
        {
            beforeTick?.Invoke();
            OnTick();
            afterTick?.Invoke();
            started = true;
            ++tick_count;
        }
        protected abstract void OnTick();

        //----------------------------------------------------------------------------------------------------------

        protected override void OnDispose()
        {
            beforeTick = afterTick = null;

            NUCLEOR.delegates.Update_OnShellTick -= Tick;

            stdout = stderr = null;

            base.OnDispose();

            status.Dispose();
        }
    }
}