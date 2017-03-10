using System;
using System.Threading.Tasks;
using DotNetFiddle.Bot.DotNetFiddleApi;
using Newtonsoft.Json;

namespace DotNetFiddle.Bot.Repl.DotNetFiddle
{
    public class DotNetFiddleReplFiddleRunner : IReplFiddleRunner
    {
        public ReplFiddleExecuteResponse ExecuteFiddle(ReplFiddleExecuteRequest request)
        {
            return ExecuteFiddleAsync(request).Result;
        }

        public async Task<ReplFiddleExecuteResponse> ExecuteFiddleAsync(ReplFiddleExecuteRequest request)
        {
            var response = new ReplFiddleExecuteResponse();

            string previousCode = String.Join(Environment.NewLine, request.PreviousCodeBlocks);
            var code = previousCode + Environment.NewLine + request.CodeBlock;

            //Execute all code
            var apiRequest = new FiddleExecuteRequest()
            {
                Compiler = Compiler.Roslyn,
                Language = Language.CSharp,
                ProjectType = ProjectType.Script,
                CodeBlock = code
            };

            DotNetFiddleApiClient apiCLient = new DotNetFiddleApiClient();
            var apiResponse = await apiCLient.ExecuteFiddleAsync(apiRequest);
            string apiError = null;

            if (apiResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                response.ExceptionErrorMessage = "There was an error calling .NET Fiddle API.";

                //Send error to errorlog or 
                apiError += "Failed to execute API request. Here is an answer from API" + Environment.NewLine;
                apiError += "Response Code: " + apiResponse.StatusCode + Environment.NewLine;
                apiError += "Response Body: " + apiResponse.Content + Environment.NewLine;
            }
            else
            {
                if (apiResponse.Data.HasErrors || apiResponse.Data.HasCompilationErrors)
                {
                    response.ExceptionErrorMessage = apiResponse.Data.ConsoleOutput;
                }
                else
                {
                    var consoleOutput = apiResponse.Data.ConsoleOutput;
                    var codeEntryConsoleOutput = consoleOutput.Substring(request.LastConsoleOutputLength); //Account for line break
                    response.LastConsoleOutputLength = consoleOutput.Length;

                    //Remove line break and other white spaces if in the beginning.  Since DNF trims output.
                    //Can fix it more later if it becomes an issue
                    response.ConsoleOutput = codeEntryConsoleOutput.TrimStart();
                }
            }


            //If worked, compare output to previous output.  Only show new lines, changes from last output


            if (request.IsDebugEnabled)
            {
                response.DebugInfo = "Input: " + code
                                                      + Environment.NewLine + Environment.NewLine
                                                      + "Output: "
                                                      + Environment.NewLine + Environment.NewLine
                                                       + JsonConvert.SerializeObject(apiResponse);

                if (apiError != null)
                    response.DebugInfo += Environment.NewLine + Environment.NewLine
                                                        + "API Error: " + Environment.NewLine
                                                        + apiError;
            }

            return response;
        }
    }
}