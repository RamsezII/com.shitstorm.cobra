using System;
using System.Collections.Generic;

namespace _COBRA_.Boa
{
    internal class AstUsrMethod_create : AstStatement
    {
        readonly MemMethod method;

        //----------------------------------------------------------------------------------------------------------

        AstUsrMethod_create(in MemMethod method)
        {
            this.method = method;
        }

        //----------------------------------------------------------------------------------------------------------

        protected internal override void OnExecutorsQueue(MemStack memstack, MemScope memscope, in Queue<Executor> executors)
        {
            base.OnExecutorsQueue(memstack, memscope, executors);

            executors.Enqueue(new(
                name: $"declare function ({method.name})",
                action_SIG_EXE: () => memscope.TrySetMethod(method.name, method)
            ));
        }

        //----------------------------------------------------------------------------------------------------------

        public static bool TryParse(in CodeReader reader, in MemScope scope, out AstUsrMethod_create ast_createMethod)
        {
            if (reader.TryReadString_match("def", false, reader.lint_theme.keywords, true, false))
                if (!reader.TryReadArgument(out string met_name, false, reader.lint_theme.functions))
                {
                    reader.CompilationError("expected new method name after operator 'def'");
                    goto failure;
                }
                else if (!reader.TryReadChar_match('('))
                {
                    reader.CompilationError("expected '(' after method name");
                    goto failure;
                }
                else
                {
                    reader.LintOpeningBraquet();

                    List<(Type type, string name)> targs = new();

                    if (reader.TryReadChar_match(')'))
                        reader.CloseBraquetLint();
                    else
                    {
                        do
                        {
                            if (!reader.TryReadArgument(out string arg_type, false, reader.lint_theme.types, stoppers: " \n\r[]{}(),;'\"\\=-*/%<>|&"))
                            {
                                reader.CompilationError($"expected a type");
                                goto failure;
                            }
                            else if (!BoaTypes.types.TryGetValue(arg_type, out Type type))
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
                        while (!reader.TryReadChar_match(')'));
                        reader.CloseBraquetLint();
                    }

                    MemScope subscope = scope.GetSubScope($"method_scope({met_name})");
                    foreach (var (type, name) in targs)
                        subscope._vars.Add(name, new MemCell(type, null));

                    if (!TryStatement(reader, subscope, out var ast_body))
                    {
                        reader.CompilationError($"expected method body");
                        goto failure;
                    }

                    MemMethod method = new(met_name, scope, ast_body, typeof(object));
                    foreach (var (type, name) in targs)
                        method.targs.Add(new MemMethod.TArgument(name, type));
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