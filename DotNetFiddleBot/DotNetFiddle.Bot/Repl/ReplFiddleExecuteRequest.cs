using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotNetFiddle.Bot.Repl
{
    public class ReplFiddleExecuteRequest
    {
        public Guid? ReplSessionId { get; set; }

        public List<string> PreviousCodeBlocks { get; set; }

        public string CodeBlock { get; set; }

        public bool IsDebugEnabled { get; set; }
        
        //To be removed when Repl is implemented in DNF
        public int LastConsoleOutputLength { get; set; }

    }
}