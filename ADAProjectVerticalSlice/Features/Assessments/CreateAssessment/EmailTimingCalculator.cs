namespace ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment
{
    public class EmailTimingCalculator
    {
        private static readonly TimeSpan RecommendedTime =
            new TimeSpan(9, 0, 0);


        public EmailTimingSuggestion Calculate(
            DateTime startDate,
            DateTime endDate)
        {
            var warnings = new List<string>();

            var invitationDate = AdjustInvitationDate(startDate, warnings);

            var headsUpDate = GetPreviousWeekday(invitationDate);

            var reminderDate = MoveToNextWeekday(invitationDate.AddDays(3));

            if (reminderDate >= endDate)
            {
                reminderDate = GetReminderBeforeEndDate(endDate);

                warnings.Add(
                    "Den foreslåede reminder ligger for tæt på " +
                    "slutdatoen og er derfor flyttet til dagen før.");
            }

            return new EmailTimingSuggestion(
                headsUpDate,
                invitationDate,
                reminderDate,
                warnings);
        }

        // Justerer invitationens dato, hvis den falder på en weekend, og tilføjer advarsler, hvis den falder på en mandag eller fredag.
        private static DateTime AdjustInvitationDate(
            DateTime startDate,
            List<string> warnings)
        {
            var invitationDate = startDate.Date
                .Add(RecommendedTime);

            if (startDate.DayOfWeek == DayOfWeek.Monday)
            {
                warnings.Add(
                    "Invitationen er planlagt til mandag. " +
                    "Overvej at starte målingen tirsdag.");
            }

            if (startDate.DayOfWeek == DayOfWeek.Friday)
            {
                warnings.Add(
                    "Invitationen er planlagt til fredag. " +
                    "Overvej at starte målingen tidligere på ugen.");
            }

            if (startDate.DayOfWeek is
                DayOfWeek.Saturday or
                DayOfWeek.Sunday)
            {
                warnings.Add(
                    "Invitationen er planlagt til en weekend " +
                    "og er derfor flyttet til mandag.");

                while (invitationDate.DayOfWeek is
                       DayOfWeek.Saturday or
                       DayOfWeek.Sunday)
                {
                    invitationDate = invitationDate.AddDays(1);
                }
            }

            return invitationDate;
        }

        // Udregner den foreslåede heads-up-dato, som er dagen før invitationen kl. 09:00 - hvis den falder på en weekend, flyttes den til den nærmeste forrige hverdag.
        private static DateTime GetPreviousWeekday(
            DateTime date)
        {
            var previousDay = date.AddDays(-1);

            while (previousDay.DayOfWeek is
                   DayOfWeek.Saturday or
                   DayOfWeek.Sunday)
            {
                previousDay = previousDay.AddDays(-1);
            }

            return previousDay;
        }

        // Udregner den foreslåede reminder-dato, som er 3 dage efter invitationen kl. 09:00 - hvis den falder på en weekend, flyttes den til den nærmeste hverdag.
        private static DateTime MoveToNextWeekday(
            DateTime date)
        {
            while (date.DayOfWeek is
                   DayOfWeek.Saturday or
                   DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }

            return date;
        }


        // Udregner den foreslåede reminder-dato, som er dagen før slutdatoen kl. 09:00 - hvis den falder på en weekend, flyttes den til den nærmeste forrige hverdag.
        private static DateTime GetReminderBeforeEndDate(
    DateTime endDate)
        {
            var reminderDate = endDate.Date
                .AddDays(-1)
                .Add(RecommendedTime);

            while (reminderDate.DayOfWeek is
                   DayOfWeek.Saturday or
                   DayOfWeek.Sunday)
            {
                reminderDate = reminderDate.AddDays(-1);
            }

            return reminderDate;
        }
    }
}
