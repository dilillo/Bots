using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo3.TimeOffBot.Models
{
    [Serializable]
    [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
    public class TimeOffRequest
    {
        [Prompt("What day will your time off start?")]
        public DateTime Start { get; set; }

        [Prompt("How many days are you requesting?")]
        [Numeric(1, 28)]
        public int Days { get; set; }

        [Prompt("And why should I approve this?")]
        public string Reason { get; set; }

        public static IForm<TimeOffRequest> Formalize()
        {
            return new FormBuilder<TimeOffRequest>()
                .Message("Welcome to the Time Off Requester Bot!  Just supply the days and I'll do the rest!")
                .AddRemainingFields()
                .Field(nameof(Reason),
                    validate: async (state, response) =>
                    {
                        var form = (TimeOffRequest)state;
                        var responseValue = (string)response;

                        if ((responseValue ?? string.Empty).Length < 5)
                            return new ValidateResult { Feedback = "You can come up with a better reason than that!" };

                        return new ValidateResult { IsValid = true, Value = responseValue };
                    })
                .Confirm("Please confirm, you want request {Days} days off starting {Start:D} because {Reason}?")
                .OnCompletionAsync((context, state) => Process(context, state))
                .Build();
        }

        private static async Task Process(IDialogContext context, TimeOffRequest state)
        {
            List<TimeOffRequest> timeOffRequests;

            if (!context.PerUserInConversationData.TryGetValue("TimeOffRequests", out timeOffRequests))
                timeOffRequests = new List<TimeOffRequest>();

            timeOffRequests.Add(state);

            context.PerUserInConversationData.SetValue("TimeOffRequests", timeOffRequests);

            await context.PostAsync("Your request has been processed.");
            await context.PostAsync("Your current time off requests are as follows: " + string.Join(", ", timeOffRequests.Select(i => $"{i.Days} days off starting {i.Start.ToShortDateString()}")));
            await context.PostAsync("Type anything to continue ...");
        }
    }
}