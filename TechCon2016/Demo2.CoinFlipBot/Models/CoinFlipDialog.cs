using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;

namespace Demo2.CoinFlipBot.Models
{
    [Serializable]
    public class CoinFlipDialog : IDialog<object>
    {
        public CoinSides Side { get; set; }

        public double Amount { get; set; }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(FirstMessageReceivedAsync);
        }

        public async Task FirstMessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            var message = await argument;

            await context.PostAsync("Welcome to the Coin Flip Game.  Good Luck!");

            PromptForChoice(context);
        }

        private void PromptForChoice(IDialogContext context)
        {
            PromptDialog.Choice(
                context,
                PromptForChoiceComplete,
                new CoinSides[] { CoinSides.Heads, CoinSides.Tails },
                $"Heads or Tails?",
                "Didn't get that!",
                promptStyle: PromptStyle.None);
        }
        
        public async Task PromptForChoiceComplete(IDialogContext context, IAwaitable<CoinSides> argument)
        {
            Side = await argument;

            PromptDialog.Number(
               context,
               PromptForAmountComplete,
               $"And how much do you want to bet?",
               "Didn't get that!");
        }

        public async Task PromptForAmountComplete(IDialogContext context, IAwaitable<double> argument)
        {
            Amount = await argument;

            var flip = Flip();

            if (flip == CoinSides.Heads && Side == CoinSides.Heads)
                await context.PostAsync("Heads it is!");
            else if (flip == CoinSides.Heads && Side == CoinSides.Tails)
                await context.PostAsync("It was Heads.  You blew it!");
            else if (flip == CoinSides.Tails && Side == CoinSides.Tails)
                await context.PostAsync("Tails it is!");
            else if (flip == CoinSides.Tails && Side == CoinSides.Heads)
                await context.PostAsync("It was Tails.  You suck!");

            var winnings = 0.0;

            context.PerUserInConversationData.TryGetValue("Winnings", out winnings);

            winnings += flip == Side ? Amount : Amount * -1;

            context.PerUserInConversationData.SetValue("Winnings", winnings);

            var downOrUp = "even";

            if (winnings < 0)
                downOrUp = $"down {winnings:c}";
            else if (winnings > 0)
                downOrUp = $"up {winnings:c}";

            await context.PostAsync($"You are {downOrUp} so far.");

            PromptForChoice(context);
        }

        public static CoinSides Flip()
        {
            var random = new Random();
            var randomNumber = random.Next();

            return randomNumber % 2 == 0 ? CoinSides.Heads : CoinSides.Tails;
        }
    }
    
    public enum CoinSides
    {
        Heads,
        Tails
    }
}