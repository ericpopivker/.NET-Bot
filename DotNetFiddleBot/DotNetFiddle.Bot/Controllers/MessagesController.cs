using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DotNetFiddle.Bot.Repl;
using Microsoft.Bot.Connector;


namespace DotNetFiddle.Bot.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));


                // calculate something for us to return
                //int length = (activity.Text ?? string.Empty).Length;

                //return our reply to the user
                //Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                var roslynSession = ReplSession.LoadOrCreate(activity.Conversation.Id);


                string replyText = String.Empty;
                string activityText = CleanActivityText(activity.Text);

                switch (activityText)
                {
                    case "#help":
                        roslynSession.Reset();
                        replyText = "\\#help - Sorry I don't have self awareness." + Environment.NewLine + Environment.NewLine;
                        replyText += "\\#reset - Reset the execution environment to the initial state, keep history.";

                        break;

                    case "#reset":
                        roslynSession.Reset();
                        replyText = "I'm melting, melting. Ohhhhh, what a world, what a world.";
                        break;

                    case "#debug":
                        roslynSession.EnableDebug();
                        break;

                    default:

                        var executeCodeEntryResult = await roslynSession.AddAndExecuteCodeEntryAsync(activityText);
                        replyText = ConvertToReplyMessage(executeCodeEntryResult);
                        break;
                }


                Activity reply = activity.CreateReply(replyText);

                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                var reply = HandleSystemMessage(activity);
                if (reply != null)
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private string ConvertToReplyMessage(ReplFiddleExecuteResponse replFiddleExecuteResponse)
        {
            string replyMesage = String.Empty;
            bool hasResultValue = (replFiddleExecuteResponse.ReturnValue != null);
            bool hasConsoleOutput = (replFiddleExecuteResponse.ConsoleOutput != null);
            bool hasExceptionErrorMessage = (replFiddleExecuteResponse.ExceptionErrorMessage != null);
            bool hasDebugInfo = (replFiddleExecuteResponse.DebugInfo != null);


            if (hasResultValue && !hasConsoleOutput)
            {
                replyMesage = replFiddleExecuteResponse.ReturnValue;
            }

            if (hasResultValue && hasConsoleOutput)
            {
                replyMesage = replFiddleExecuteResponse.ReturnValue;
                replyMesage += Environment.NewLine + Environment.NewLine;
                replyMesage += "[Console] " + replFiddleExecuteResponse.ConsoleOutput;
            }

            if (!hasResultValue && hasConsoleOutput)
            {
                replyMesage = replFiddleExecuteResponse.ConsoleOutput;
            }

            if (hasExceptionErrorMessage)
            {
                replyMesage = "[Exception] " + replFiddleExecuteResponse.ExceptionErrorMessage;
            }

            if (!hasResultValue && !hasConsoleOutput && !hasExceptionErrorMessage)
                replyMesage = GetEmptyReplyPlaceholder();

            if (hasDebugInfo)
            {
                replyMesage += Environment.NewLine + Environment.NewLine;
                replyMesage += "[Debug] ";
                replyMesage += Environment.NewLine + Environment.NewLine;
                replyMesage += replFiddleExecuteResponse.DebugInfo;
            }

            return replyMesage;
        }

        private string CleanActivityText(string activityText)
        {
            // In group chat Bot messages may have prefix:
            // <at id="28:e1854b75-e713-4c26-910d-5778763c3739">@.NET REPL Bot</at>


            string botHandle = "@" + ConfigurationManager.AppSettings["BotId"] + "</at>";

            int index = activityText.IndexOf(botHandle);

            if (index != -1)
                activityText = activityText.Remove(0, index + botHandle.Length);

            activityText = activityText.Trim();

            //In group chat Skype replaces quotes with &quot;
            activityText = activityText.Replace("&quot;", @"""");
            activityText = activityText.Replace("&amp;", @"&");

            return activityText;
        }


        static Random _rnd = new Random();

        private static List<string> _emptyPlaceholders = new List<string>
        {
            "...",
            "No output detected.",
            "I got nothing.",
            "... and then?",
            "Roses are red, violets are blue, I got no output for you."
        };



        private string GetEmptyReplyPlaceholder()
        {
            int index = _rnd.Next(_emptyPlaceholders.Count);
            return _emptyPlaceholders[index];
        }


        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                if (message.MembersAdded != null)
                {
                    string replyText = "I am .NET Bot (v 0.9). ";
                    replyText += "I like compilers, syntax trees and long walks on the beach." + Environment.NewLine + Environment.NewLine;
                    replyText += "Type '#help' for more information.";

                    Activity reply = message.CreateReply(replyText);
                    reply.Recipient = message.MembersAdded[0];

                    return reply;
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened

            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing that the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}