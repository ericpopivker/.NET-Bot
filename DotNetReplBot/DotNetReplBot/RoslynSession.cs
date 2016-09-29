using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web.Caching;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace DotNetReplBot
{
    public class RoslynSession
    {
        private List<string> _codeEntries;
        private bool _isDebugEnabled;

        public string SessionId { get; set; }

        public List<string> CodeEntries
        {
            get
            {
                return _codeEntries;
            }
            
        }

        private RoslynSession(string sessionId)
        {
            _codeEntries = new List<string>();

            SessionId = sessionId;
        }


        public static RoslynSession Create(string sessionId)
        {
            var session = new RoslynSession(sessionId);
            session.CodeEntries.Add("using System;");
            session.AddToCache();
            return session;

        }


        public static RoslynSession LoadOrCreate(string sessionId)
        {
            var session = Load(sessionId);
            if (session == null)
                session = Create(sessionId);
            return session;
        }


        public static RoslynSession Load(string sessionId)
        {
            var obj = System.Web.HttpRuntime.Cache.Get(sessionId);
            return obj as RoslynSession;
        }

        public static void Remove(Guid sessionId)
        {
            System.Web.HttpRuntime.Cache.Remove(sessionId.ToString());
        }

        private void AddToCache()
        {
            string key = SessionId.ToString();
            System.Web.HttpRuntime.Cache.Add(key, this, null, Cache.NoAbsoluteExpiration,  new TimeSpan(1, 0, 0), CacheItemPriority.High, null);
        }

        private Object _lockLastEntryExecution = new Object();
        private ScriptState _scriptState;

        public class ExecuteCodeEntryResult
        {
            public string ConsoleOutput { get; set; }

            public string ReturnValue { get; set; }

            public string ExceptionErrorMessage { get; set; }

            public string DebugInfo { get; set; }
        }

        public async Task<ExecuteCodeEntryResult> AddAndExecuteCodeEntryAsync(string code)
        {
            ExecuteCodeEntryResult result = new ExecuteCodeEntryResult();

            _scriptState = null;

            _codeEntries.Add(code);

            for (int i = 0; i < _codeEntries.Count; i++)
            {
                bool isLastCodeEntry = i == _codeEntries.Count - 1;
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
                                result.ReturnValue = RunCodeEntry(_codeEntries[i]).Result;

                            }
                            catch (Exception ex)
                            {
                                //User inner exception because async method throws AggregateException
                                result.ExceptionErrorMessage = ex.InnerException.Message;
                            }


                            string consoleOutputText = newConsoleOutput.ToString();

                            if (!String.IsNullOrEmpty(consoleOutputText))
                                result.ConsoleOutput = consoleOutputText;
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
                        string someReturnValue = await RunCodeEntry(_codeEntries[i]);
                    }
                    catch
                    {
                       // Do nada
                    }
                }
            }

            if (_isDebugEnabled)
            {
                result.DebugInfo = "Input: " + code
                                  + Environment.NewLine + Environment.NewLine
                                  + "Output: "
                                  + Environment.NewLine + Environment.NewLine 
                                   + JsonConvert.SerializeObject(result);
            }

            return result;
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




        public void Reset()
        {
            _codeEntries.Clear();
            _isDebugEnabled = false;
        }

        public void EnableDebug()
        {
            _isDebugEnabled = true;
        }
    }
}