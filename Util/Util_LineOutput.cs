using System;

namespace _COBRA_
{
    public enum CMDLINE_STATUS
    {
        _none_,
        CONFIRM,
        REJECT,
        ERROR,
    }

    [Serializable]
    public readonly struct CMDLINE_DATA
    {
        public readonly CMDLINE_STATUS status;
        public readonly object data;
        public override string ToString() => $"{GetType().FullName} {nameof(status)}: {status} ({nameof(data)}: {data})";

        //--------------------------------------------------------------------------------------------------------------

        internal CMDLINE_DATA(in CMDLINE_STATUS status, in object data = null)
        {
            this.status = status;
            this.data = data;
        }
    }
}