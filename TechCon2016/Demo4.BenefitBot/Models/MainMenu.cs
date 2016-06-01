using Microsoft.Bot.Builder.FormFlow;
using System;

namespace Demo4.BenefitBot.Models
{
    [Serializable]
    [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
    public class MainMenu
    {
        public MainMenuOptions Selection { get; set; }

        public static IForm<MainMenu> Formalize()
        {
            return new FormBuilder<MainMenu>()
                .Message("Hi! Benni here, what can I do for you today?")
                .AddRemainingFields()
                .Build();
        }
    }

    public enum MainMenuOptions
    {
        ReviewYourBenefits = 1,
        ChangeYourBenefits,
        Logout
    }
}