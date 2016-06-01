using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System;
using System.Threading.Tasks;

namespace Demo4.BenefitBot.Models
{
    [Serializable]
    [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
    public class Enrollment
    {
        public string MedicalPlan { get; set; }

        public MedicalTiers MedicalTier { get; set; }

        public double HsaPlanContribution { get; set; }

        public string DentalPlan { get; set; }

        public DentalTiers DentalTier { get; set; }
        
        public static IForm<Enrollment> Formalize()
        {
            return new FormBuilder<Enrollment>()
                .Message("Let's get your benefits updated!")
                .Field(new FieldReflector<Enrollment>(nameof(Enrollment.MedicalPlan))
                    .SetType(null)
                    .SetPrompt(new PromptAttribute("What Medical coverage would you like? {||}"))
                    .SetDefine(async (state, field) =>
                    {
                        field
                            .AddDescription("waive", "Waive Medical Benefits")
                            .AddTerms("waive", "waive", "waive medical")
                            .AddDescription("ppo", "PPO Medical Plan")
                            .AddTerms("ppo", "ppo", "ppo medical")
                            .AddDescription("hd", "High Deductible Medical Plan")
                            .AddTerms("hd", "hd", "hd medical");

                        return true;
                    }))
                .Field(new FieldReflector<Enrollment>(nameof(Enrollment.MedicalTier))
                    .SetPrompt(new PromptAttribute("Who would you like to cover with your Medical plan? {||}"))
                    .SetActive(i => i.MedicalPlan != "waive"))
                .Field(nameof(Enrollment.HsaPlanContribution), "How much would you like to contribute to your HSA plan each paycheck?",
                    active: (state) =>
                    {
                        return state.MedicalPlan == "hd";
                    },
                    validate: async (state, response) =>
                    {
                        var form = (Enrollment)state;
                        var responseValue = (double)response;

                        if (responseValue < 0.0d)
                            return new ValidateResult { Feedback = "You can not contribute a negative amount." };

                        if (responseValue > 100d)
                            return new ValidateResult { Feedback = "You can contribute up to $100 per pay without exceeding your IRS yearly limit." };

                        return new ValidateResult { IsValid = true, Value = responseValue };
                    })
                .Field(new FieldReflector<Enrollment>(nameof(Enrollment.DentalPlan))
                    .SetType(null)
                    .SetPrompt(new PromptAttribute("What Dental coverage would you like? {||}"))
                    .SetDefine(async (state, field) =>
                    {
                        field
                            .AddDescription("waive", "Waive Dental Benefits")
                            .AddTerms("waive", "waive", "waive dental")
                            .AddDescription("group", "Group Dental Plan")
                            .AddTerms("group", "group", "group dental");

                        return true;
                    }))
                .Field(new FieldReflector<Enrollment>(nameof(Enrollment.DentalTier))
                    .SetPrompt(new PromptAttribute("Who would you like to cover with your Dental plan? {||}"))
                    .SetActive(i => i.DentalPlan != "waive"))
                .AddRemainingFields()
                .Message("Thanks for enrolling!  Send any response to continue  ...")
                .OnCompletionAsync((context, state) => Process(context, state))
                .Build();
        }

        private static async Task Process(IDialogContext context, Enrollment state)
        {
            context.PerUserInConversationData.SetValue("Enrollment", state);
        }

        public static string Summarize(Enrollment enrollment)
        {
            var medicalPlan = "No Medical";

            if (enrollment.MedicalPlan == "ppo")
                medicalPlan = "PPO Medical";
            else if (enrollment.MedicalPlan == "hd")
                medicalPlan = $"High Deductible Medical with {enrollment.HsaPlanContribution:C} going into your HSA each pay";

            var medicalTier = "nobody";

            if (enrollment.MedicalTier == MedicalTiers.EmployeeOnly)
                medicalTier = "you only";
            else if (enrollment.MedicalTier == MedicalTiers.EmployeePlusFamily)
                medicalTier = "you + family";
            else if (enrollment.MedicalTier == MedicalTiers.EmployeePlusSpouse)
                medicalTier = "you + spouse";

            var dentalPlan = "No Dental";

            if (enrollment.DentalPlan == "group")
                dentalPlan = "Group Dental";

            var dentalTier = "nobody";

            if (enrollment.DentalTier == DentalTiers.EmployeeOnly)
                medicalTier = "you only";
            else if (enrollment.DentalTier == DentalTiers.EmployeePlusFamily)
                medicalTier = "you + family";

            return $"You are currently enrolled in {medicalPlan} covering {medicalTier} and {dentalPlan} covering {dentalTier}.  Send any response to continue  ...";
        }
    }

    public enum MedicalTiers
    {
        EmployeeOnly = 1,
        EmployeePlusSpouse,
        EmployeePlusFamily
    }

    public enum DentalTiers
    {
        EmployeeOnly = 1,
        EmployeePlusFamily
    }
}