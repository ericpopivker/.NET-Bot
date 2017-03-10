using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetFiddle.Bot.DotNetFiddleApi;
using NUnit.Framework;

namespace DotNetFiddle.Bot.Tests
{
    [TestFixture]
    public class DotNetFiddleApiClientTest
    {
        [Test]
        public void ExecuteFiddle_When_valid_code_Then_returns_result()
        {
            DotNetFiddleApiClient dotNetFiddleApiClient = new DotNetFiddleApiClient();

            var request = new FiddleExecuteRequest()
            {
                Compiler = Compiler.Roslyn,
                Language = Language.CSharp,
                ProjectType = ProjectType.Script,
                CodeBlock = "Console.WriteLine(\"Hello World\");"
            };

            var response = dotNetFiddleApiClient.ExecuteFiddle(request); 
        }


        [Test]
        public void ExecuteFiddleAsync_When_valid_code_Then_returns_result()
        {
            DotNetFiddleApiClient dotNetFiddleApiClient = new DotNetFiddleApiClient();

            var request = new FiddleExecuteRequest()
            {
                Compiler = Compiler.Roslyn,
                Language = Language.CSharp,
                ProjectType = ProjectType.Script,
                CodeBlock = "Console.WriteLine(\"Hello World\");"
            };

            var response = dotNetFiddleApiClient.ExecuteFiddleAsync(request).Result;
        }



        [Test]
        public void ExecuteFiddleAsync_When_script_fiddle_with_console_output_and_return_value_Then_returns_both()
        {
            DotNetFiddleApiClient dotNetFiddleApiClient = new DotNetFiddleApiClient();

            string codeBlock = @"using System;
					
Console.WriteLine(""Hello World"");

int i = 2 + 2;
i
";

            var request = new FiddleExecuteRequest()
            {
                Compiler = Compiler.Roslyn,
                Language = Language.CSharp,
                ProjectType = ProjectType.Script,
                CodeBlock = codeBlock
            };

            var response = dotNetFiddleApiClient.ExecuteFiddleAsync(request).Result;
            Assert.AreEqual("Hello World\r\n[Return value]: 4", response.Data.ConsoleOutput);

        }
    }
}
