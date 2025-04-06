using UnityEngine;

namespace _COBRA_
{
    public enum CMD_SIGNALS : byte
    {
        _NONE_,
        LINT,
        CHECK,
        EXEC,
        ALT_DOUBLE,
        TAB,
        ALT_UP,
        ALT_DOWN,
        ALT_LEFT,
        ALT_RIGHT,
    }

    public enum CMD_STATES : byte
    {
        DONE,
        BLOCKING,
        WAIT_FOR_STDIN,
        FULLSCREEN_w,
        FULLSCREEN_r,
    }

    public struct CMD_STATUS
    {
        public CMD_STATES state;
        public string prefixe;
        [Range(0, 1)] public float progress;
        public string error;
    }
}