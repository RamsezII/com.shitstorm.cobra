using _UTIL_;
using System;

namespace _COBRA_.Boa
{
    static class AstPrimary
    {
        public static bool TryPrimary(in CodeReader reader, in TScope tscope, in Type expected_type, out AstExpression ast_factor)
        {
            if (reader.sig_error == null)
                if (expected_type == Util_cobra.T_path)
                    if (reader.TryParsePath(FS_TYPES.BOTH, false, out string path))
                    {
                        ast_factor = new AstLiteral<string>(path);
                        return true;
                    }

            if (reader.sig_error == null)
                if (expected_type == Util_cobra.T_fpath)
                    if (reader.TryParsePath(FS_TYPES.FILE, false, out string fpath))
                    {
                        ast_factor = new AstLiteral<string>(fpath);
                        return true;
                    }

            if (reader.sig_error == null)
                if (expected_type == Util_cobra.T_dpath)
                    if (reader.TryParsePath(FS_TYPES.DIRECTORY, false, out string dpath))
                    {
                        ast_factor = new AstLiteral<string>(dpath);
                        return true;
                    }

            if (reader.sig_error == null)
                if (reader.TryReadChar_match('('))
                {
                    reader.LintOpeningBraquet();
                    if (!AstExpression.TryExpr(reader, tscope, false, typeof(object), out ast_factor))
                    {
                        reader.Error("expected expression inside factor parenthesis.");
                        goto failure;
                    }
                    else if (!reader.TryReadChar_match(')', lint: reader.CloseBraquetLint()))
                    {
                        reader.Error($"expected closing parenthesis ')' after factor.");
                        --reader.read_i;
                        goto failure;
                    }
                    else
                        return true;
                }

            if (reader.sig_error == null)
                if (expected_type == null || typeof(string).IsAssignableFrom(expected_type))
                    if (AstString.TryParseString(reader, tscope, out var ast_string))
                    {
                        ast_factor = ast_string;
                        return true;
                    }
                    else if (reader.sig_error != null)
                        goto failure;

            if (reader.sig_error == null)
                if (AstContract.TryParseContract(reader, tscope, expected_type, out var ast_contract))
                {
                    ast_factor = ast_contract;
                    return true;
                }
                else if (reader.sig_error != null)
                    goto failure;
                else if (AstVariable.TryParseVariable(reader, tscope, expected_type, out var ast_variable))
                {
                    ast_factor = ast_variable;
                    return true;
                }
                else if (reader.sig_error != null)
                    goto failure;
                else if (reader.TryReadArgument(out string arg, lint: reader.lint_theme.fallback_default, as_function_argument: false, stoppers: CodeReader._stoppers_factors_))
                    switch (arg.ToLower())
                    {
                        case "true":
                            reader.LintToThisPosition(reader.lint_theme.constants, true);
                            ast_factor = new AstLiteral<bool>(true);
                            return true;

                        case "false":
                            reader.LintToThisPosition(reader.lint_theme.constants, true);
                            ast_factor = new AstLiteral<bool>(false);
                            return true;

                        default:
                            if (arg[^1] == 'f' && Util.TryParseFloat(arg[..^1], out float _float))
                                ast_factor = new AstLiteral<float>(_float);
                            else if (int.TryParse(arg, out int _int))
                                ast_factor = new AstLiteral<int>(_int);
                            else if (Util.TryParseFloat(arg, out _float))
                                ast_factor = new AstLiteral<float>(_float);
                            else
                            {
                                reader.Error($"unrecognized literal : '{arg}'.");
                                goto failure;
                            }
                            reader.LintToThisPosition(reader.lint_theme.literal, true);
                            return true;
                    }

                failure:
            ast_factor = null;
            return false;
        }
    }
}