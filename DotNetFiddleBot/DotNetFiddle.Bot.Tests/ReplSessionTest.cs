using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DotNetFiddle.Bot.Tests
{
    [TestFixture]
    public class ReplSessionTest
    {
        [Test]
        public void AddAndExecuteCodeEntryAsync_When_valid_code_entry_Then_returns_expected_output()
        {
            var sessionId = Guid.NewGuid().ToString();
            var replSession = ReplSession.Create(sessionId);

            string code = "Console.WriteLine(\"Hello World\");";


            var task = replSession.AddAndExecuteCodeEntryAsync(code);
            var result = task.Result;

            Assert.AreEqual("Hello World", result.ConsoleOutput);
        }


        [Test]
        public void AddAndExecuteCodeEntryAsync_When_second_valid_code_entry_Then_returns_output_only_for_second_entry()
        {
            var sessionId = Guid.NewGuid().ToString();
            var replSession = ReplSession.Create(sessionId);

            string code = "Console.WriteLine(\"Hello World\");" +
                          "Console.WriteLine(\"  \");";

            var task = replSession.AddAndExecuteCodeEntryAsync(code);
            var result = task.Result;


            code = "Console.WriteLine(\"Hello Universe\");";
            task = replSession.AddAndExecuteCodeEntryAsync(code);
            result = task.Result;
            
            Assert.AreEqual("Hello Universe", result.ConsoleOutput);
        }
    }
}

