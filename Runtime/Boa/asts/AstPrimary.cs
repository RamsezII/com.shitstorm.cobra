using _UTIL_;
using System;

namespace _COBRA_.Boa
{
    static class AstPrimary
    {
        public static bool TryPrimary(in CodeReader reader, in MemScope scope, in Type expected_type, out AstExpression ast_factor)
        {
            ast_factor = null;

            if (expected_type == typeof(BoaPath))
                if (reader.TryParsePath(FS_TYPES.BOTH, false, out string path))
                {
                    ast_factor = new AstLiteral(path);
                    return true;
                }

            if (reader.StopParsing())
                goto failure;

            if (expected_type == typeof(BoaFPath))
                if (reader.TryParsePath(FS_TYPES.FILE, false, out string fpath))
                {
                    ast_factor = new AstLiteral(fpath);
                    return true;
                }

            if (reader.StopParsing())
                goto failure;

            if (expected_type == typeof(BoaDPath))
                if (reader.TryParsePath(FS_TYPES.DIRECTORY, false, out string dpath))
                {
                    ast_factor = new AstLiteral(dpath);
                    return true;
                }

            if (reader.ContinueParsing())
                if (ast_factor != null)
                {
                    int read_old_accessor = reader.read_i;
                    if (reader.TryReadPrefixeString_match("->"))
                    {
                        reader.LintToThisPosition(reader.lint_theme.operators, true);
                        var ast_old = ast_factor;

                        if (ast_factor.output_type != null)
                            if (AstField.TryField(reader, ast_factor, out var ast_accessor))
                                ast_factor = ast_accessor;
                            else if (reader.sig_error != null)
                                goto failure;
                            else
                                reader.read_i = read_old_accessor;

                        if (ast_factor.output_type != null)
                            if (AstMethod.TryMethod(reader, scope, ast_factor, out var ast_accessor))
                                ast_factor = ast_accessor;
                            else if (reader.sig_error != null)
                                goto failure;
                            else
                                reader.read_i = read_old_accessor;

                        if (ast_old != ast_factor)
                        {
                            reader.CompilationError($"expected field or method name after operator '->'");
                            ast_factor = null;
                            goto failure;
                        }
                    }
                }

            if (reader.StopParsing())
                goto failure;

            if (reader.TryReadChar_match('('))
            {
                reader.LintOpeningBraquet();
                if (!AstExpression.TryExpr(reader, scope, false, typeof(object), out ast_factor))
                {
                    reader.CompilationError("expected expression inside factor parenthesis.");
                    goto failure;
                }
                else if (!reader.TryReadChar_match(')', lint: reader.CloseBraquetLint()))
                {
                    reader.CompilationError($"expected closing parenthesis ')' after factor.");
                    --reader.read_i;
                    goto failure;
                }
                else
                    return true;
            }

            if (reader.StopParsing())
                goto failure;

            if (expected_type == null || expected_type.IsAssignableFrom(typeof(string)))
                if (AstString.TryParseString(reader, scope, out var ast_string))
                {
                    ast_factor = ast_string;
                    return true;
                }
                else if (reader.sig_error != null)
                    goto failure;

            if (reader.StopParsing())
                goto failure;

            if (AstContract.TryParseContract(reader, scope, expected_type, out var ast_contract))
            {
                ast_factor = ast_contract;
                return true;
            }
            else if (reader.sig_error != null)
                goto failure;

            if (reader.StopParsing())
                goto failure;

            if (expected_type != null)
                if (DevTypes.TryDevType(reader, expected_type, out ast_factor))
                    return true;

            if (AstVariable.TryParseVariable(reader, scope, expected_type, out var ast_variable))
            {
                ast_factor = ast_variable;
                return true;
            }
            else if (reader.sig_error != null)
                goto failure;

            if (reader.StopParsing())
                goto failure;

            if (reader.TryReadArgument(out string arg, lint: reader.lint_theme.fallback_default, as_function_argument: false))
                switch (arg.ToLower())
                {
                    case "true":
                        reader.LintToThisPosition(reader.lint_theme.constants, true);
                        ast_factor = new AstLiteral(true);
                        return true;

                    case "false":
                        reader.LintToThisPosition(reader.lint_theme.constants, true);
                        ast_factor = new AstLiteral(false);
                        return true;

                    default:
                        if (arg[^1] == 'f' && Util.TryParseFloat(arg[..^1], out float _float))
                            ast_factor = new AstLiteral(_float);
                        else if (int.TryParse(arg, out int _int))
                            ast_factor = new AstLiteral(_int);
                        else if (Util.TryParseFloat(arg, out _float))
                            ast_factor = new AstLiteral(_float);
                        else
                        {
                            reader.CompilationError($"unrecognized literal : '{arg}'.");
                            goto failure;
                        }
                        reader.LintToThisPosition(reader.lint_theme.literal, true);
                        return true;
                }

            failure:
            return reader.sig_error == null && ast_factor != null;
        }
    }
}