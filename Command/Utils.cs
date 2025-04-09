using System;
using UnityEngine;

namespace _COBRA_
{
    enum CMD_SIGNALS_enum : byte
    {
        _kill,
        _save,
        _lint,
        _check,
        _exec,
        _double,
        _cpl,
        _tab,
        _alt,
        _up,
        _down,
        _left,
        _right,
        _last_
    }

    [Flags]
    public enum CMD_SIGNALS : ushort
    {
        SAVE = 1 << CMD_SIGNALS_enum._save,
        LINT = 1 << CMD_SIGNALS_enum._lint,
        CHECK = 1 << CMD_SIGNALS_enum._check,
        EXEC = 1 << CMD_SIGNALS_enum._exec,
        DOUBLE = 1 << CMD_SIGNALS_enum._double,
        CPL = 1 << CMD_SIGNALS_enum._cpl,
        TAB = 1 << CMD_SIGNALS_enum._tab,
        ALT = 1 << CMD_SIGNALS_enum._alt,
        UP = 1 << CMD_SIGNALS_enum._up,
        RIGHT = 1 << CMD_SIGNALS_enum._right,
        DOWN = 1 << CMD_SIGNALS_enum._down,
        LEFT = 1 << CMD_SIGNALS_enum._left,

        CPL_TAB = CPL | TAB,
        CPL_ALT = CPL | ALT,

        ALT_UP = CPL_ALT | UP,
        ALT_RIGHT = CPL_ALT | RIGHT,
        ALT_DOWN = CPL_ALT | DOWN,
        ALT_LEFT = CPL_ALT | LEFT,
    }

    public enum CMD_STATES : byte
    {
        DONE,
        BLOCKING,
        WAIT_FOR_STDIN,
        FULLSCREEN_readonly,
        FULLSCREEN_write,
    }

    public struct CMD_STATUS
    {
        public CMD_STATES state;
        public string prefixe;
        public bool immortal;
        [Range(0, 1)] public float progress;
        public string error;

        //--------------------------------------------------------------------------------------------------------------

        public CMD_STATUS(in CMD_STATES state,
            in string prefixe = null,
            in bool immortal = false,
            in float progress = 0,
            in string error = null)
        {
            this.state = state;
            this.prefixe = prefixe;
            this.immortal = immortal;
            this.progress = progress;
            this.error = error;
        }
    }
}