using System;

namespace _COBRA_
{
    enum SIGNALS_enum : byte
    {
        _kill,
        _save,
        _lint,
        _validate,
        _check,
        _exec,
        _thick,
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
    public enum SIGNALS : ushort
    {
        _none_,
        KILL = 1 << SIGNALS_enum._kill,
        SAVE = 1 << SIGNALS_enum._save,
        LINT = 1 << SIGNALS_enum._lint,
        VALIDATE = 1 << SIGNALS_enum._validate,
        CHECK = 1 << SIGNALS_enum._check,
        EXEC = 1 << SIGNALS_enum._exec,
        TICK = 1 << SIGNALS_enum._thick,
        DOUBLE = 1 << SIGNALS_enum._double,
        CPL = 1 << SIGNALS_enum._cpl,
        TAB = 1 << SIGNALS_enum._tab,
        ALT = 1 << SIGNALS_enum._alt,
        UP = 1 << SIGNALS_enum._up,
        RIGHT = 1 << SIGNALS_enum._right,
        DOWN = 1 << SIGNALS_enum._down,
        LEFT = 1 << SIGNALS_enum._left,

        CPL_TAB = CPL | TAB,
        CPL_ALT = CPL | ALT,

        ALT_UP = CPL_ALT | UP,
        ALT_RIGHT = CPL_ALT | RIGHT,
        ALT_DOWN = CPL_ALT | DOWN,
        ALT_LEFT = CPL_ALT | LEFT,

        STDIN_CHANGE = LINT | VALIDATE,
    }
}