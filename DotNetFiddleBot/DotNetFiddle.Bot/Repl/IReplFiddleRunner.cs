using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DotNetFiddle.Bot.Repl
{
    public interface IReplFiddleRunner
    {
        ReplFiddleExecuteResponse ExecuteFiddle(ReplFiddleExecuteRequest request);

        Task<ReplFiddleExecuteResponse> ExecuteFiddleAsync(ReplFiddleExecuteRequest request);
    }
}