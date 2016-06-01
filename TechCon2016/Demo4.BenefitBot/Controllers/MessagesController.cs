using Demo4.BenefitBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace Demo4.BenefitBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        internal static IDialog<Object> MakeRootDialog()
        {
            return Chain
               .From(() => FormDialog.FromForm(MainMenu.Formalize))
               .Switch(
                    Chain.Case<MainMenu, IDialog<Object>>
                    (
                        i => i.Selection == MainMenuOptions.ChangeYourBenefits,
                        (ctx, sr) => Chain.From(() => FormDialog.FromForm(Enrollment.Formalize, FormOptions.PromptInStart))
                    ),
                    Chain.Case<MainMenu, IDialog<Object>>
                    (
                        i => i.Selection == MainMenuOptions.ReviewYourBenefits,
                        (ctx, sr) =>
                        {
                            Enrollment enrollment;

                            if (!ctx.PerUserInConversationData.TryGetValue("Enrollment", out enrollment))
                                return Chain.PostToUser(Chain.Return("You are not currently enrolled in benefits.  Send any response to continue ..."));

                            return Chain.PostToUser(Chain.Return(Enrollment.Summarize(enrollment)));
                        }
                    ),
                    Chain.Default<MainMenu, IDialog<Object>>
                    (
                        (ctx, sr) => Chain.From(() => FormDialog.FromForm(Enrollment.Formalize, FormOptions.PromptInStart))
                    )
                ).Unwrap().Loop();
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                return await Conversation.SendAsync(message, MakeRootDialog);
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}