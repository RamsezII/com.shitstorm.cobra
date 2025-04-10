using System;

namespace _COBRA_
{
    enum SIGNAL_ENUM : byte
    {
        _kill,
        _save,
        _lint,
        _validate,
        _check,
        _tick,
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
    public enum SIGNAL_FLAGS : ushort
    {
        _none_,
        KILL = 1 << SIGNAL_ENUM._kill,
        SAVE = 1 << SIGNAL_ENUM._save,
        LINT = 1 << SIGNAL_ENUM._lint,
        VALIDATE = 1 << SIGNAL_ENUM._validate,
        CHECK = 1 << SIGNAL_ENUM._check,
        TICK = 1 << SIGNAL_ENUM._tick,
        EXEC = 1 << SIGNAL_ENUM._exec,
        DOUBLE = 1 << SIGNAL_ENUM._double,
        CPL = 1 << SIGNAL_ENUM._cpl,
        TAB = 1 << SIGNAL_ENUM._tab,
        ALT = 1 << SIGNAL_ENUM._alt,
        UP = 1 << SIGNAL_ENUM._up,
        RIGHT = 1 << SIGNAL_ENUM._right,
        DOWN = 1 << SIGNAL_ENUM._down,
        LEFT = 1 << SIGNAL_ENUM._left,

        CPL_TAB = CPL | TAB,
        CPL_ALT = CPL | ALT,

        ALT_UP = CPL_ALT | UP,
        ALT_RIGHT = CPL_ALT | RIGHT,
        ALT_DOWN = CPL_ALT | DOWN,
        ALT_LEFT = CPL_ALT | LEFT,

        STDIN_CHANGE = LINT | VALIDATE,
    }
}