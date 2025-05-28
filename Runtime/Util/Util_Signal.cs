using System;

namespace _COBRA_
{
    enum _SigFlags : byte
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
    public enum SIG_FLAGS : ushort
    {
        _none_,
        KILL = 1 << _SigFlags._kill,
        SAVE = 1 << _SigFlags._save,
        LINT = 1 << _SigFlags._lint,
        CHECK = 1 << _SigFlags._check,
        EXEC = 1 << _SigFlags._exec,
        TICK = 1 << _SigFlags._tick,
        DOUBLE = 1 << _SigFlags._double,
        CPL = 1 << _SigFlags._cpl,
        TAB = CPL | 1 << _SigFlags._tab,
        LIST = CPL | 1 << _SigFlags._list,
        ALT = CPL | 1 << _SigFlags._alt,
        HIST = 1 << _SigFlags._hist,
        UP = 1 << _SigFlags._up,
        RIGHT = 1 << _SigFlags._right,
        DOWN = 1 << _SigFlags._down,
        LEFT = 1 << _SigFlags._left,

        ALT_UP = ALT | UP,
        ALT_RIGHT = ALT | RIGHT,
        ALT_DOWN = ALT | DOWN,
        ALT_LEFT = ALT | LEFT,

        HIST_UP = HIST | UP,
        HIST_DOWN = HIST | DOWN,
    }
}