using System;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal class AstCreateMethod : AstStatement
    {
        readonly MemMethod method;

        //----------------------------------------------------------------------------------------------------------

        AstCreateMethod(in MemMethod method)
        {
            this.method = method;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(executors);

            executors.Enqueue(new(
                name: $"declare function ({method.name})",
                action_SIG_EXE: janitor => janitor.shell.scope.TrySetMethod(method.name, method)
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryParse(in CodeReader reader, in MemScope scope, out AstCreateMethod ast_createMethod)
        {
            if (reader.TryReadString_match("def", false, reader.lint_theme.keywords, true, false))
                if (reader.TryReadArgument(out string met_name, false, reader.lint_theme.functions))
                {
                    bool got_braquet = reader.TryReadChar_match('(');
                    if (got_braquet)
                        reader.LintOpeningBraquet();

                    char stopchar = got_braquet ? ')' : ':';
                    List<(Type, string)> targs = new();

                    if (!reader.TryPeekChar_match(stopchar, out _))
                        do
                        {
                            if (!reader.TryReadArgument(out string arg_type, false, reader.lint_theme.types, stoppers: " \n\r[]{}(),;'\"\\=-*/%<>|&"))
                            {
                                reader.CompilationError($"expected a type");
                                goto failure;
                            }
                            else if (!Util.TryGetType(arg_type, out Type type, include_abstracts: true))
                            {
                                reader.CompilationError($"could not parse type \"{arg_type}\"");
                                goto failure;
                            }
                            else if (!reader.TryReadArgument(out string arg_name, true, reader.lint_theme.argument))
                            {
                                reader.CompilationError($"expected name after type \"{type}\"");
                                goto failure;
                            }
                            else
                                targs.Add((type, arg_name));
                        }
                        while (!reader.TryReadChar_match(stopchar));

                    if (got_braquet)
                        reader.CloseBraquetLint();
                    else
                        reader.LintToThisPosition(reader.lint_theme.operators, true);

                    if (!TryStatement(reader, scope, out var body))
                    {
                        reader.CompilationError($"expected method body");
                        goto failure;
                    }

                    MemMethod method = new(met_name, body, typeof(object));
                    scope.TrySetMethod(met_name, method);
                    ast_createMethod = new(method);

                    return true;
                }

            failure:
            ast_createMethod = null;
            return false;
        }
    }
}