using NUnit.Framework.Constraints;
using System;

namespace _COBRA_
{
    partial class CmdVars
    {
        enum Operators : byte
        {
            equal,
            not_equal,
            greater,
            less,
            greater_or_equal,
            less_or_equal,
            add,
            subtract,
            multiply,
            divide,
            modulo,
        }

        //--------------------------------------------------------------------------------------------------------------

        static void Init_Compare()
        {
            Command.static_domain.AddAction(
                "math-int",
                min_args: 3,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg0, out bool seems_valid, strict: true, completions: Enum.GetNames(typeof(Operators))))
                        if (Enum.TryParse(arg0, true, out Operators code))
                        {
                            exe.args.Add(code);
                            if (exe.line.TryReadArgument(out string arg1, out _))
                            {
                                exe.args.Add(arg1);
                                if (exe.line.TryReadArgument(out string arg2, out _))
                                    exe.args.Add(arg2);
                            }
                        }
                        else
                            exe.error = $"invalid comparator: \'{arg0}\'";
                },
                action: static (exe) =>
                {
                    Operators code = (Operators)exe.args[0];
                    string s1 = exe.args[1].ToString();
                    string s2 = exe.args[2].ToString();

                    if (code == Operators.equal || code == Operators.not_equal)
                    {
                        bool equal = s1.Equals(s2, StringComparison.Ordinal);
                        exe.Stdout(equal == (code == Operators.equal));
                    }
                    else
                    {
                        if (!int.TryParse(s1, out int i1))
                            exe.error = $"could not parse '{s1}' to {typeof(int)}";
                        else if (!int.TryParse(s2, out int i2))
                            exe.error = $"could not parse '{s2}' to an {typeof(int)}";
                        else
                            exe.Stdout(code switch
                            {
                                Operators.greater => i1 > i2,
                                Operators.less => i1 < i2,
                                Operators.greater_or_equal => i1 >= i2,
                                Operators.less_or_equal => i1 <= i2,
                                Operators.add => i1 + i2,
                                Operators.subtract => i1 - i2,
                                Operators.multiply => i1 * i2,
                                Operators.divide => i1 / i2,
                                Operators.modulo => i1 % i2,
                                _ => throw new ArgumentException("unimplemented", typeof(Operators).FullName),
                            });
                    }
                });

            Command.static_domain.AddAction(
                "math-float",
                min_args: 3,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg0, out bool seems_valid, strict: true, completions: Enum.GetNames(typeof(Operators))))
                        if (Enum.TryParse(arg0, true, out Operators code))
                        {
                            exe.args.Add(code);
                            if (exe.line.TryReadArgument(out string arg1, out _))
                            {
                                exe.args.Add(arg1);
                                if (exe.line.TryReadArgument(out string arg2, out _))
                                    exe.args.Add(arg2);
                            }
                        }
                        else
                            exe.error = $"invalid comparator: \'{arg0}\'";
                },
                action: static (exe) =>
                {
                    Operators code = (Operators)exe.args[0];
                    string s1 = exe.args[1].ToString();
                    string s2 = exe.args[2].ToString();

                    if (code == Operators.equal || code == Operators.not_equal)
                    {
                        bool equal = s1.Equals(s2, StringComparison.Ordinal);
                        exe.Stdout(equal == (code == Operators.equal));
                    }
                    else
                    {
                        if (!Util.TryParseFloat(s1, out float f1))
                            exe.error = $"could not parse '{s1}' to {typeof(float)}";
                        else if (!Util.TryParseFloat(s2, out float f2))
                            exe.error = $"could not parse '{s2}' to {typeof(float)}";
                        else
                            exe.Stdout(code switch
                            {
                                Operators.greater => f1 > f2,
                                Operators.less => f1 < f2,
                                Operators.greater_or_equal => f1 >= f2,
                                Operators.less_or_equal => f1 <= f2,
                                Operators.add => f1 + f2,
                                Operators.subtract => f1 - f2,
                                Operators.multiply => f1 * f2,
                                Operators.divide => f1 / f2,
                                Operators.modulo => f1 % f2,
                                _ => throw new ArgumentException("unimplemented", typeof(Operators).FullName),
                            });
                    }
                });
        }
    }
}