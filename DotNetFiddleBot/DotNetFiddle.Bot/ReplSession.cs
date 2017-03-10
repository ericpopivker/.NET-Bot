using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web.Caching;
using DotNetFiddle.Bot.DotNetFiddleApi;
using DotNetFiddle.Bot.Repl;
using DotNetFiddle.Bot.Repl.DotNetFiddle;
using DotNetFiddle.Bot.Repl.Roslyn;
using Newtonsoft.Json;

namespace DotNetFiddle.Bot
{
    public class ReplSession
    {
        private List<string> _codeBlocks;
        private bool _isDebugEnabled;
        private int _lastConsoleOutputLength;

        private IReplFiddleRunner _replFiddleRunner;
        public string SessionId { get; set; }

        public List<string> CodeBlocks
        {
            get
            {
                return _codeBlocks;
            }
            
        }

        public int LastConsoleOutputLength
        {
            get
            {
                return _lastConsoleOutputLength;
            }

        }
        private ReplSession(string sessionId, IReplFiddleRunner replFiddleRunner = null)
        {
            _codeBlocks = new List<string>();
            _lastConsoleOutputLength = 0;

            SessionId = sessionId;

            if (replFiddleRunner == null)
                _replFiddleRunner = new RoslynReplFiddleRunner();
            else
                _replFiddleRunner = replFiddleRunner;
        }


        private const string DefaultIncludes = @"using System;
using static System.Console;
using System.Text;";

        public static ReplSession Create(string sessionId, IReplFiddleRunner replFiddleRunner = null)
        {
            var session = new ReplSession(sessionId, replFiddleRunner);
            session.CodeBlocks.Add(DefaultIncludes);
            session.AddToCache();
            return session;
        }


        public static ReplSession LoadOrCreate(string sessionId)
        {
            var session = Load(sessionId);
            if (session == null)
                session = Create(sessionId);
            return session;
        }


        public static ReplSession Load(string sessionId)
        {
            var obj = System.Web.HttpRuntime.Cache.Get(sessionId);
            return obj as ReplSession;
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


        public async Task<ReplFiddleExecuteResponse> AddAndExecuteCodeEntryAsync(string codeBlock)
        {
            var replFiddleExecuteRequest = new ReplFiddleExecuteRequest
            {
                PreviousCodeBlocks = _codeBlocks,
                CodeBlock = codeBlock,
                IsDebugEnabled =  _isDebugEnabled,
                LastConsoleOutputLength = _lastConsoleOutputLength
            };


            var response = await _replFiddleRunner.ExecuteFiddleAsync(replFiddleExecuteRequest);


         
            if (String.IsNullOrEmpty(response.ExceptionErrorMessage))
            {
                _lastConsoleOutputLength = response.LastConsoleOutputLength;
                _codeBlocks.Add(codeBlock);
            }

            return response;
        }


        public void Reset()
        {
            _codeBlocks.Clear();
            _lastConsoleOutputLength = 0;

            _isDebugEnabled = false;
        }

        public void EnableDebug()
        {
            _isDebugEnabled = true;
        }
    }
}