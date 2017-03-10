using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetFiddle.Bot.Repl;
using DotNetFiddle.Bot.Repl.Roslyn;
using NUnit.Framework;

namespace DotNetFiddle.Bot.Tests
{
    [TestFixture]
    public class RoslynReplFiddleRunnerTest
    {
        [Test]
        public void ExecuteFiddle_When_valid_code_entry_Then_returns_expected_output()
        {
            var runner = new RoslynReplFiddleRunner();
            
            string code = "Console.WriteLine(\"Hello World\");";

            var request = new ReplFiddleExecuteRequest()
            {
                PreviousCodeBlocks = GetDefaultCodeBlocks(),
                CodeBlock = code
            };

            var response = runner.ExecuteFiddle(request);
            Assert.AreEqual("Hello World", response.ConsoleOutput);
        }

        private const string DefaultIncludes = @"using System;
using static System.Console;
using System.Text;";

        private List<string> GetDefaultCodeBlocks()
        {
            var codeBlocks = new List<string>();
            codeBlocks.Add(DefaultIncludes);

            return codeBlocks;
        }

        [Test]
        public void AddAndExecuteCodeEntryAsync_When_second_valid_code_entry_Then_returns_output_only_for_second_entry()
        {
            var runner = new RoslynReplFiddleRunner();

            string code = "Console.WriteLine(\"Hello World\");" +
                         "Console.WriteLine(\"  \");";

            var codeBlocks = GetDefaultCodeBlocks();
            codeBlocks.Add(code);

            var request = new ReplFiddleExecuteRequest()
            {
                PreviousCodeBlocks = codeBlocks,
                CodeBlock = "Console.WriteLine(\"Hello Universe\");"
            };
            
            var response = runner.ExecuteFiddle(request);
            Assert.AreEqual("Hello Universe", response.ConsoleOutput);
        }
    }
}

