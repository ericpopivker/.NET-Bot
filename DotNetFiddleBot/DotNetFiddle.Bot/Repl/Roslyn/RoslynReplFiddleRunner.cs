using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace DotNetFiddle.Bot.Repl.Roslyn
{
    public class RoslynReplFiddleRunner : IReplFiddleRunner
    {
        private Object _lockLastEntryExecution = new Object();
        private ScriptState _scriptState;


        public ReplFiddleExecuteResponse ExecuteFiddle(ReplFiddleExecuteRequest request)
        {

            return ExecuteFiddleAsync(request).Result;
        }

        public async Task<ReplFiddleExecuteResponse> ExecuteFiddleAsync(ReplFiddleExecuteRequest request)
        {
            var response = new ReplFiddleExecuteResponse();

            _scriptState = null;

           List<string> codeBlocks = new List<string>();
           codeBlocks.AddRange(request.PreviousCodeBlocks);
           codeBlocks.Add(request.CodeBlock);


            for (int i = 0; i < codeBlocks.Count; i++)
            {
                bool isLastCodeEntry = i == codeBlocks.Count - 1;
                TextWriter oldConsoleOutput = null;
                TextWriter newConsoleOutput = null;

                if (isLastCodeEntry)
                {
                    lock (_lockLastEntryExecution)
                    {
                        oldConsoleOutput = Console.Out;
                        newConsoleOutput = new StringWriter();
                        Console.SetOut(newConsoleOutput);

                        try
                        {

                            try
                            {
                                response.ReturnValue = RunCodeEntry(codeBlocks[i]).Result;

                            }
                            catch (Exception ex)
                            {
                                //User inner exception because async method throws AggregateException
                                response.ExceptionErrorMessage = ex.InnerException.Message;
                            }


                            string consoleOutputText = newConsoleOutput.ToString();

                            if (!String.IsNullOrEmpty(consoleOutputText))
                                response.ConsoleOutput = consoleOutputText.Trim();
                        }
                        finally
                        {
                            Console.SetOut(oldConsoleOutput);
                        }
                    }
                }
                else
                {
                    try
                    {
                        //The return values for all but last code entry are not used
                        string someReturnValue = await RunCodeEntry(codeBlocks[i]);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }

            if (request.IsDebugEnabled)
            {
                response.DebugInfo = "Input: " + request.CodeBlock
                                  + Environment.NewLine + Environment.NewLine
                                  + "Output: "
                                  + Environment.NewLine + Environment.NewLine
                                   + JsonConvert.SerializeObject(response);
            }

            return response;
        }




        private async Task<string> RunCodeEntry(string code)
        {
            string returnValue = null;


            if (_scriptState == null)
                _scriptState = await CSharpScript.RunAsync(code);
            else
                _scriptState = await _scriptState.ContinueWithAsync(code);

            if (_scriptState.ReturnValue != null)
                returnValue = _scriptState.ReturnValue.ToString();

            return returnValue;
        }
    }
}
