using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetFiddle.Bot.Repl
{
    public class ReplFiddleExecuteResponse
    {
        public string ConsoleOutput { get; set; }

        public string ReturnValue { get; set; }

        public string ExceptionErrorMessage { get; set; }

        public string DebugInfo { get; set; }

        //To be removed when Repl is implemented in DNF
        public int LastConsoleOutputLength { get; set; }
    }
}