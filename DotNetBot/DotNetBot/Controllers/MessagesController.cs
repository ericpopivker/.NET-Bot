using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace DotNetBot
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
                var roslynSession = RoslynSession.LoadOrCreate(activity.Conversation.Id);


                string replyText = String.Empty;

                switch (activity.Text)
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

                    default:
                        string returnValue = await roslynSession.AddAndExecuteCodeEntryAsync(activity.Text);
                        if (returnValue == null)
                            returnValue = GetEmptyPlaceholder();
                        replyText = returnValue;
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


        static Random _rnd = new Random();

        private static List<string> _emptyPlaceholders = new List<string>
        {
            "Hmmm...",
            "Zzzzz...",
            "Make up yoir mind, friend.",
            "Wait let me call Billy G...",
            "I got nothing."
        };



        private string GetEmptyPlaceholder()
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