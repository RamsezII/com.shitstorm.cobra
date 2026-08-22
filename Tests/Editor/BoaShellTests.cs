using _COBRA_.Boa;
using NUnit.Framework;
using System;
using System.IO;

namespace _COBRA_.Tests
{
    public sealed class BoaShellTests
    {
        sealed class AccessTarget
        {
            public readonly int value;

            public AccessTarget(in int value)
            {
                this.value = value;
            }
        }

        static CodeReader Reader(in SIG_FLAGS flags, in string text) => new(
            sig_flags: flags,
            workdir: Directory.GetCurrentDirectory(),
            lint_theme: null,
            strict_syntax: false,
            text: text,
            script_path: null
        );

        [Test]
        public void ChangeParsingDoesNotMutateRuntimeVariablesOrLeakScopes()
        {
            BoaShell shell = new("change_parse_test");

            try
            {
                shell.scope._vars.Add("x", new MemCell(5));

                CodeReader reader = Reader(SIG_FLAGS.CHANGE | SIG_FLAGS.LINT, "x += 1");
                shell.OnReader(reader);

                Assert.That(reader.sig_error, Is.Null);
                Assert.That(shell.scope._vars["x"]._value, Is.EqualTo(5));
                Assert.That(shell.scope.ChildCount, Is.Zero);
            }
            finally
            {
                shell.Dispose();
            }
        }

        [Test]
        public void NotEqualIsAComparisonAndDoesNotAssign()
        {
            BoaShell shell = new("not_equal_test");
            object output = null;

            try
            {
                shell.scope._vars.Add("x", new MemCell(1));
                shell.scope._vars.Add("y", new MemCell(2));
                shell.stdout += (value, _) => output = value;

                CodeReader reader = Reader(SIG_FLAGS.SUBMIT, "x != y");
                shell.OnReader(reader);
                shell.Tick();

                Assert.That(reader.sig_error, Is.Null);
                Assert.That(output, Is.EqualTo(true));
                Assert.That(shell.scope._vars["x"]._value, Is.EqualTo(1));
            }
            finally
            {
                shell.Dispose();
            }
        }

        [Test]
        public void ArrowAccessorExecutesRegisteredField()
        {
            const string field_name = "cobra_test_value";
            DevField.all_fields.Remove(typeof(AccessTarget));
            DevField<AccessTarget, int>.AddAttribute(new(
                name: field_name,
                onExecution: static (stack, _, target) => stack.Add(new MemCell(target.value))
            ));

            BoaShell shell = new("accessor_test");
            object output = null;

            try
            {
                shell.scope._vars.Add("target", new MemCell(new AccessTarget(42)));
                shell.stdout += (value, _) => output = value;

                CodeReader reader = Reader(SIG_FLAGS.SUBMIT, $"target->{field_name}");
                shell.OnReader(reader);
                shell.Tick();

                Assert.That(reader.sig_error, Is.Null);
                Assert.That(output, Is.EqualTo(42));
            }
            finally
            {
                shell.Dispose();
                DevField.all_fields.Remove(typeof(AccessTarget));
            }
        }

        [Test]
        public void NullCellsCompareWithoutThrowing()
        {
            MemCell left = new(typeof(object), null);
            MemCell right = new(typeof(object), null);

            Assert.DoesNotThrow(() => _ = left == right);
            Assert.That((bool)(left == right), Is.True);
        }
    }
}
