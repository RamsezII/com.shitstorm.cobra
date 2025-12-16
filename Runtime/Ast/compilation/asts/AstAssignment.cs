using _COBRA_.Boa.execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _COBRA_.Boa.compilation
{
    internal class AstAssignment : AstExpression
    {
        enum Codes : byte
        {
            Default,
            Incr,
            Decr,
            Mult,
            Divide,
            Modulo,
            And,
            Or,
            Xor,
            No,
        }

        static readonly Dictionary<string, Codes> codes = new(StringComparer.OrdinalIgnoreCase)
        {
            { "=", Codes.Default },
            { "!=", Codes.No },
            { "+=", Codes.Incr },
            { "-=", Codes.Decr },
            { "*=", Codes.Mult },
            { "/=", Codes.Divide },
            { "%=", Codes.Modulo },
            { "&=", Codes.And },
            { "|=", Codes.Or },
            { "^=", Codes.Xor },
        };

        readonly string var_name;
        readonly Type var_type;
        readonly AstExpression expr_value;
        readonly Codes code;

        //----------------------------------------------------------------------------------------------------------

        AstAssignment(in string var_name, in Codes code, in AstExpression expression, in Type output_type) : base(output_type)
        {
            this.var_name = var_name;
            expr_value = expression;
            this.code = code;
        }

        //----------------------------------------------------------------------------------------------------------

        internal static bool TryParseAssignment(in CodeReader reader, in TScope tscope, in Type expected_type, out AstExpression ast_assignation)
        {
            int read_old = reader.read_i;

            if (reader.TryReadString_matches_out(
                out string var_name,
                as_function_argument: false,
                lint: reader.lint_theme.variables,
                matches: tscope.EVarNames().ToArray())
            )
                if (!tscope.TryGet(var_name, out Type var_type))
                {
                    reader.Error($"no variable named '{var_name}' declared");
                    goto failure;
                }
                else if (expected_type != null && !expected_type.IsAssignableFrom(var_type))
                {
                    reader.Error($"excepted {expected_type}, got {var_type}");
                    goto failure;
                }
                else
                {
                    reader.LintToThisPosition(reader.lint_theme.variables, true);

                    if (!reader.TryReadString_matches_out(
                        value: out string op_name,
                        as_function_argument: false,
                        lint: reader.lint_theme.operators,
                        ignore_case: true,
                        add_to_completions: true,
                        skippables: CodeReader._empties_,
                        stoppers: " \n\r{}(),;'\"",
                        matches: codes.Keys)
                    )
                        goto failure;
                    else
                    {
                        Codes code = codes[op_name];

                        if (TryExpr(reader, tscope, false, var_type, out AstExpression expr))
                        {
                            ast_assignation = new AstAssignment(var_name, code, expr, var_type);
                            return true;
                        }
                        else
                        {
                            reader.Error($"expected expression after '{op_name}' operator.");
                            goto failure;
                        }
                    }
                }

            failure:
            reader.read_i = read_old;
            ast_assignation = null;
            return false;
        }

        //----------------------------------------------------------------------------------------------------------

        internal override void OnExecutionStack(in Janitor janitor)
        {
            base.OnExecutionStack(janitor);

            //object value = expr_exe.output;

            //if (code != OP_CODES.ASSIGN)
            //{
            //    if (var_cell.value is bool var_b && value is bool expr_b)
            //        value = code switch
            //        {
            //            OP_CODES.AND => var_b && expr_b,
            //            OP_CODES.OR => var_b || expr_b,
            //            OP_CODES.XOR => var_b != expr_b,
            //            _ => throw new NotSupportedException(),
            //        };
            //    else if (var_cell.value is int var_i && value is int expr_i)
            //        value = code switch
            //        {
            //            OP_CODES.ADD => var_i + expr_i,
            //            OP_CODES.SUBSTRACT => var_i - expr_i,
            //            OP_CODES.MULTIPLY => var_i * expr_i,
            //            OP_CODES.DIVIDE => var_i / expr_i,
            //            OP_CODES.MODULO => var_i % expr_i,
            //            OP_CODES.AND => var_i & expr_i,
            //            OP_CODES.OR => var_i | expr_i,
            //            _ => throw new NotSupportedException(),
            //        };
            //    else if (var_cell.value is float var_f && value is float expr_f)
            //        value = code switch
            //        {
            //            OP_CODES.ADD => var_f + expr_f,
            //            OP_CODES.SUBSTRACT => var_f - expr_f,
            //            OP_CODES.MULTIPLY => var_f * expr_f,
            //            OP_CODES.DIVIDE => var_f / expr_f,
            //            OP_CODES.MODULO => var_f % expr_f,
            //            _ => throw new NotSupportedException(),
            //        };
            //    else if (var_cell.value is string var_s)
            //        value = code switch
            //        {
            //            OP_CODES.ADD => var_s + value,
            //            _ => throw new NotSupportedException(),
            //        };
            //}

            //var_cell.value = value;
        }
    }
}