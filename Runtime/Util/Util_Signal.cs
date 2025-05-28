using System;

namespace _COBRA_
{
    enum SIGNALS_enum : byte
    {
        _kill,
        _save,
        _lint,
        _check,
        _exec,
        _tick,
        _double,
        _cpl,
        _tab,
        _list,
        _alt,
        _hist,
        _up,
        _down,
        _left,
        _right,
        _last_,
    }

    [Flags]
    public enum SIGNALS : ushort
    {
        _none_,
        KILL = 1 << SIGNALS_enum._kill,
        SAVE = 1 << SIGNALS_enum._save,
        LINT = 1 << SIGNALS_enum._lint,
        CHECK = 1 << SIGNALS_enum._check,
        EXEC = 1 << SIGNALS_enum._exec,
        TICK = 1 << SIGNALS_enum._tick,
        DOUBLE = 1 << SIGNALS_enum._double,
        CPL = 1 << SIGNALS_enum._cpl,
        TAB = CPL | 1 << SIGNALS_enum._tab,
        LIST = CPL | 1 << SIGNALS_enum._list,
        ALT = CPL | 1 << SIGNALS_enum._alt,
        HIST = 1 << SIGNALS_enum._hist,
        UP = 1 << SIGNALS_enum._up,
        RIGHT = 1 << SIGNALS_enum._right,
        DOWN = 1 << SIGNALS_enum._down,
        LEFT = 1 << SIGNALS_enum._left,

        ALT_UP = ALT | UP,
        ALT_RIGHT = ALT | RIGHT,
        ALT_DOWN = ALT | DOWN,
        ALT_LEFT = ALT | LEFT,

        HIST_UP = HIST | UP,
        HIST_DOWN = HIST | DOWN,
    }
}