using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Caching;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DotNetBot
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


        public async Task<string> AddAndExecuteCodeEntryAsync(string code)
        {
            string returnValue = String.Empty;
            ScriptState state = null;

            _codeEntries.Add(code);

            for (int i = 0; i < _codeEntries.Count; i++)
            {
                try
                {
                    state = await RunCodeEntry(_codeEntries[i], state);
                    returnValue = state.ReturnValue?.ToString();
                }
                catch (Exception ex)
                {
                    returnValue = ex.Message;
                }
            }

            return returnValue;
        }

        private async Task<ScriptState> RunCodeEntry(string code, ScriptState state)
        {
            if (state == null)
                state = await CSharpScript.RunAsync(code);
            else
                state = await state.ContinueWithAsync(code);

            return state;
        }


        public void Reset()
        {
            _codeEntries.Clear();
        }
    }
}