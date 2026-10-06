using ADA_Contracts;
using ADA_Contracts.Enums;
using ADA_Contracts.Events;
using ADA_EmailConsumer.Messaging;

namespace ADA_EmailConsumer.Consumers;

public class AssessmentCreatedConsumer
{
    private readonly RabbitMqEmailScheduler _scheduler;
    private readonly ILogger<AssessmentCreatedConsumer> _logger;

    public AssessmentCreatedConsumer(
        RabbitMqEmailScheduler scheduler,
        ILogger<AssessmentCreatedConsumer> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task Handle(
        AssessmentCreated @event,
        CancellationToken cancellationToken)
    {
        // TEST MODE - SCHEDULE EMAILS MED 5, 10 OG 15 SEKUNDER I STEDET FOR DE RIGTIGE DATOER
        _logger.LogInformation(
            "TEST MODE: AssessmentCreated received for Assessment {AssessmentId}",
            @event.AssessmentId);

        // HEADS UP - 5 sekunder
        var headsUp = new EmailScheduled(
            @event.AssessmentId,
            EmailType.HeadsUp,
            @event.ApplicationName,
            @event.StartDate,
            @event.EndDate,
            @event.HeadsUpDate,
            @event.Respondents);

        await _scheduler.ScheduleAsync(
            headsUp,
            TimeSpan.FromSeconds(5),
            cancellationToken);

        _logger.LogInformation(
            "TEST: Heads-up scheduled in 5 seconds.");

        // INVITATION - 10 sekunder
        var invitation = new EmailScheduled(
            @event.AssessmentId,
            EmailType.Invitation,
            @event.ApplicationName,
            @event.StartDate,
            @event.EndDate,
            @event.InvitationDate,
            @event.Respondents);

        await _scheduler.ScheduleAsync(
            invitation,
            TimeSpan.FromSeconds(10),
            cancellationToken);

        _logger.LogInformation(
            "TEST: Invitation scheduled in 10 seconds.");

        // REMINDER - 15 sekunder
        var reminder = new EmailScheduled(
            @event.AssessmentId,
            EmailType.Reminder,
            @event.ApplicationName,
            @event.StartDate,
            @event.EndDate,
            @event.ReminderDate,
            @event.Respondents);

        await _scheduler.ScheduleAsync(
            reminder,
            TimeSpan.FromSeconds(15),
            cancellationToken);

        _logger.LogInformation(
            "TEST: Reminder scheduled in 15 seconds.");

        ///// IKKE SLET! DETTE ER DEN KORREKTE IMPLEMENTERING TIL AT SCHEDULE EMAILS, MEN DEN ER KOMMENTERET UD SÅ VI KAN TESTE FORSINKELSE MED KORTERE TID. /////
        //_logger.LogInformation(
        //    "AssessmentCreated received for Assessment {AssessmentId}",
        //    @event.AssessmentId);

        //// HEADS UP
        //var headsUp = new EmailScheduled(
        //    @event.AssessmentId,
        //    EmailType.HeadsUp,
        //    @event.ApplicationName,
        //    @event.StartDate,
        //    @event.EndDate,
        //    @event.HeadsUpDate,
        //    @event.Respondents);

        //var headsUpDelay =
        //    @event.HeadsUpDate - DateTime.UtcNow;

        //await _scheduler.ScheduleAsync(
        //    headsUp,
        //    headsUpDelay,
        //    cancellationToken);

        //_logger.LogInformation(
        //    "Heads-up email scheduled for {ScheduledDate}",
        //    @event.HeadsUpDate);


        //// INVITATION
        //var invitation = new EmailScheduled(
        //    @event.AssessmentId,
        //    EmailType.Invitation,
        //    @event.ApplicationName,
        //    @event.StartDate,
        //    @event.EndDate,
        //    @event.InvitationDate,
        //    @event.Respondents);

        //var invitationDelay =
        //    @event.InvitationDate - DateTime.UtcNow;

        //await _scheduler.ScheduleAsync(
        //    invitation,
        //    invitationDelay,
        //    cancellationToken);

        //_logger.LogInformation(
        //    "Invitation email scheduled for {ScheduledDate}",
        //    @event.InvitationDate);


        //// REMINDER
        //var reminder = new EmailScheduled(
        //    @event.AssessmentId,
        //    EmailType.Reminder,
        //    @event.ApplicationName,
        //    @event.StartDate,
        //    @event.EndDate,
        //    @event.ReminderDate,
        //    @event.Respondents);

        //var reminderDelay =
        //    @event.ReminderDate - DateTime.UtcNow;

        //await _scheduler.ScheduleAsync(
        //    reminder,
        //    reminderDelay,
        //    cancellationToken);

        //_logger.LogInformation(
        //    "Reminder email scheduled for {ScheduledDate}",
        //    @event.ReminderDate);
    }
}

