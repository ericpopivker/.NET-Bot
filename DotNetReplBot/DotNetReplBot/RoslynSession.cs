using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web.Caching;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DotNetReplBot
{
    public class RoslynSession
    {
        private List<string> _codeEntries;

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

        public async Task<string> AddAndExecuteCodeEntryAsync(string code)
        {
            string returnValue = String.Empty;
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

                            //This one have to do sync because of lock
                            returnValue = RunCodeEntry(_codeEntries[i]).Result;


                            string consoleOutputText = newConsoleOutput.ToString();

                            if (!String.IsNullOrEmpty(consoleOutputText))
                            {
                                if (returnValue != null)
                                {
                                    returnValue += Environment.NewLine + "[Console] " + consoleOutputText;
                                }
                                else
                                {
                                    returnValue = consoleOutputText;
                                }
                            }

                        }
                        finally
                        {
                            Console.SetOut(oldConsoleOutput);
                        }
                    }

                }
                else
                {
                    returnValue = await RunCodeEntry(_codeEntries[i]);
                }
                
            }

            return returnValue;
        }



        
        private async Task<string> RunCodeEntry(string code)
        {
            string returnValue = null;

            try
            {
                if (_scriptState == null)
                    _scriptState = await CSharpScript.RunAsync(code);
                else
                    _scriptState = await _scriptState.ContinueWithAsync(code);

                if (_scriptState.ReturnValue != null)
                    returnValue = _scriptState.ReturnValue.ToString();
            }
            catch (Exception ex)
            {
                returnValue = ex.Message;
            }

            return returnValue;
        }




        public void Reset()
        {
            _codeEntries.Clear();
        }
    }
}