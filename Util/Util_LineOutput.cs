using System;

namespace _COBRA_
{
    public enum CMDLINE_STATUS
    {
        _none_,
        OK,
        REFUSE,
        ERROR,
    }

    [Serializable]
    public struct CMDLINE_DATA
    {
        public CMDLINE_STATUS status;
        public object data;
    }
}