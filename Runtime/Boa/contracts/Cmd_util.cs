using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _COBRA_.Boa.contracts
{
    static class Cmd_util
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            DevContract.AddContract(new(
                name: "typeof",
                output_type: typeof(Type),
                arguments: new() { typeof(object), },
                action: static (janitor, prms) =>
                {
                    MemCell cell = prms.arguments[0];
                    Type type = cell._type;
                    janitor.vstack.Add(new(type));
                }
            ));

            DevContract.AddContract(new(
                name: "check_script",
                arguments: new() { typeof(BoaFPath), },
                action: static (janitor, prms) =>
                {
                    string fpath = (string)prms.arguments[0]._value;
                    string text = File.ReadAllText(fpath);

                    CodeReader reader = new(SIG_FLAGS.CHECK, janitor.shell.workdir._value, null, false, text, fpath);
                    MemScope scope = new();
                    Queue<AstAbstract> asts = new();

                    while (reader.HasNext() && AstStatement.TryStatement(reader, scope, out var ast))
                        if (ast != null)
                            asts.Enqueue(ast);

                    bool execute_in_background = reader.TryReadChar_match('&', lint: reader.lint_theme.command_separators);

                    if (reader.TryPeekChar_out(out char peek, out _))
                        reader.CompilationError($"could not parse everything ({nameof(peek)}: '{peek}').");

                    if (reader.sig_error != null)
                    {
                        reader.LocalizeError();
                        janitor.shell.on_output(reader.sig_long_error, reader.sig_long_error.SetColor(Color.orange));
                    }
                    else
                        janitor.shell.on_output("no compilation error", null);
                }
            ));

            DevContract.AddContract(new(
                name: "run_script",
                arguments: new() { typeof(BoaFPath), },
                action: static (janitor, prms) =>
                {
                    string fpath = (string)prms.arguments[0]._value;
                    string text = File.ReadAllText(fpath);

                    CodeReader reader = new(SIG_FLAGS.CHECK, janitor.shell.workdir._value, null, false, text, fpath);
                    MemScope scope = new();
                    Queue<AstAbstract> asts = new();

                    while (reader.HasNext() && AstStatement.TryStatement(reader, scope, out var ast))
                        if (ast != null)
                            asts.Enqueue(ast);

                    bool execute_in_background = reader.TryReadChar_match('&', lint: reader.lint_theme.command_separators);

                    if (reader.TryPeekChar_out(out char peek, out _))
                        reader.CompilationError($"could not parse everything ({nameof(peek)}: '{peek}').");

                    if (reader.sig_error != null)
                    {
                        reader.LocalizeError();
                        janitor.shell.on_output(reader.sig_long_error, reader.sig_long_error.SetColor(Color.red));
                    }
                    else
                        foreach (var ast in asts)
                            ast.OnExecutorsQueue(janitor.executors);
                }
            ));
        }
    }
}