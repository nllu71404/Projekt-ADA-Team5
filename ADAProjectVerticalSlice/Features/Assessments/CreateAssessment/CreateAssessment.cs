using Carter;
using MediatR;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ADAProjectAPIVerticalSlice.Shared;
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Infrastructure.Messaging;
using ADA_Contracts.Events;
using ADA_Contracts.Other_contracts;
using System.Security.Cryptography;

namespace ADAProjectAPIVerticalSlice.Features.Assessments.CreateAssessment
{
    public class CreateAssessment
    {
        //COMMAND = En Command beskriver en handling, som systemet skal udføre (CQRS pattern).
        //IRequest<Result<Guid>> betyder, at Command'en sendes gennem MediatR og forventer et Result<Guid> tilbage.
        public class Command : IRequest<Result<Guid>>
        {
            public string AssessmentName { get; set; } = string.Empty;

            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }

            public string ApplicationName { get; set; } = string.Empty;

            public Guid SurveyId { get; set; } 

            public List<string> RoleNames { get; set; } = new List<string>();

            public List<Guid> RegionIds { get; set; } = new List<Guid>();

            public List<Guid> ExperienceIds { get; set; } = new List<Guid>();
            public List<string> RespondentEmails { get; set; } = new List<string>();




        }


        // FluentValidation bruges til at definere reglerne på en deklarativ måde i stedet for at skrive if-statements direkte i Handleren.
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.AssessmentName).NotEmpty()
                .WithMessage("Målingsnavnet må ikke være tomt.");
                RuleFor(c => c.EndDate)
                    .GreaterThan(c => c.StartDate)
                    .WithMessage("Slutdato skal være efter startdato.");
                RuleFor(c => c.RoleNames).NotEmpty()
                    .WithMessage("Mindst én rolle skal være valgt.");
                RuleFor(c => c.RegionIds).NotEmpty()
                    .WithMessage("Mindst én region skal være valgt.");
                RuleFor(c => c.ExperienceIds).NotEmpty()
                    .WithMessage("Mindst ét erfaringsinterval skal være valgt.");
                RuleFor(c => c.ApplicationName).NotEmpty()
                    .WithMessage("Applikationsnavnet må ikke være tomt.");
            }
        }

        // HANDLER = Handleren indeholder den konkrete logik for Command'en.
        // Denne Handler håndterer CreateAssessment.Command og returnerer et Result<Guid>.
        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDbContext _dbContext;

            // Validatoren bliver injected via Dependency Injection. Den bruges til at kontrollere, om Command'en er gyldig.
            private readonly IValidator<Command> _validator;

            private readonly IRabbitMqPublisher _publisher;

            public Handler(
                ApplicationDbContext dbContext,
                IValidator<Command> validator,
                IRabbitMqPublisher publisher)
            {
                _dbContext = dbContext;
                _validator = validator;
                _publisher = publisher;
            }

            // genererer en tilfældig adgangstoken for Respondent-objekter.
            private static string GenerateAccessToken()
            {
                return Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(32));
            }

            //Henter de valgte Regions i databasen
            private async Task<Result<List<Region>>> GetRegions(List<Guid> regionIds, CancellationToken cancellationToken)
            {
                var regions = await _dbContext.Regions
                    .Where(r => regionIds.Contains(r.RegionId))
                    .ToListAsync(cancellationToken);

                if (regions.Count != regionIds.Count)
                {
                    return Result.Failure<List<Region>>(
                        new Error(
                            "CreateAssessment.RegionNotFound",
                            "Regioner kunne ikke findes."));
                }

                return regions;
            }

            //Henter de valgte Experiences i databasen
            private async Task<Result<List<Experience>>> GetExperiences(List<Guid> experienceIds, CancellationToken cancellationToken)
            {
                var experiences = await _dbContext.Experiences
                    .Where(e => experienceIds.Contains(e.ExperienceId))
                    .ToListAsync(cancellationToken);
                if (experiences.Count != experienceIds.Count)
                {
                    return Result.Failure<List<Experience>>(
                        new Error(
                            "CreateAssessment.ExperienceNotFound",
                            "Erfaringer kunne ikke findes."));
                }
                return experiences;
            }

            // Handle() bliver automatisk kaldt af MediatR, når en CreateAssessment.Command bliver sendt.
            public async Task<Result<Guid>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var validationResult =  _validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return Result.Failure<Guid>(new Error(
                        "CreateAssessment.Validation",
                        validationResult.ToString()));
                }

                // Find eller opret Application
                var application = await _dbContext.Applications
                    .FirstOrDefaultAsync(
                        a => a.ApplicationName == request.ApplicationName,
                        cancellationToken);

                if (application == null)
                {
                    application = new Application
                    {
                        ApplicationId = Guid.NewGuid(),
                        ApplicationName = request.ApplicationName
                    };

                    _dbContext.Applications.Add(application);
                }

                // Find eller opret Roles
                var roles = new List<Role>();

                foreach (var roleName in request.RoleNames)
                {
                    var role = await _dbContext.Roles
                        .FirstOrDefaultAsync(
                            r => r.RoleName == roleName,
                            cancellationToken);

                    if (role == null)
                    {
                        role = new Role
                        {
                            RoleId = Guid.NewGuid(),
                            RoleName = roleName
                        };

                        _dbContext.Roles.Add(role);
                    }

                    roles.Add(role);
                }


                // Hent de valgte Regions og Experiences fra databasen
                var regionsResult = await GetRegions(request.RegionIds,cancellationToken);

                if (regionsResult.IsFailure)
                {
                    return Result.Failure<Guid>(regionsResult.Error);
                }

                var experiencesResult = await GetExperiences(request.ExperienceIds, cancellationToken);

                if (experiencesResult.IsFailure)
                {
                    return Result.Failure<Guid>(experiencesResult.Error);
                }

                // Hent den aktuelle bruger fra databasen. Vi skal ændre denne til at hente den aktuelle bruger fra konteksten (f.eks. via JWT token eller session).
                var currentUser = await _dbContext.Users.FirstOrDefaultAsync(
                    u => u.Email == "test@test.dk",
                    cancellationToken);

                if (currentUser is null)
                {
                    return Result.Failure<Guid>(Error.NullValue);
                }

                // Hent Survey fra databasen. Vi bruger en hardcoded SurveyId for nu, men dette skal ændres til at hente den korrekte SurveyId fra requesten.
                var surveyId = Guid.Parse("22222222-2222-2222-2222-222222222222");

                var survey = await _dbContext.Surveys
                    .FirstOrDefaultAsync(
                        s => s.SurveyId == surveyId,
                        cancellationToken);

                if (survey is null)
                {
                    return Result.Failure<Guid>(
                        new Error(
                            "CreateAssessment.SurveyNotFound",
                            "ADA Survey could not be found."));
                }


                // Opret Assessment objektet og sæt de nødvendige properties
                var assessment = new Assessment
                {
                    AssessmentId = Guid.NewGuid(),
                    AssessmentName = request.AssessmentName,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ApplicationId = application.ApplicationId,
                    SurveyId = survey.SurveyId,
                    Roles = roles,
                    Experiences = experiencesResult.Value,
                    Regions = regionsResult.Value,
                  

                    //Skal komme fra den autentificerede bruger, som sender requesten. 
                    //Dette kræver, at vi har en mekanisme til at hente den aktuelle bruger fra konteksten (f.eks. via JWT token eller session).
                    UserId = currentUser.Id 
                };

               

                // Tilføj Assessment til databasen
                _dbContext.Assessments.Add(assessment);

                // Gem Assessment, eventuelle nye Applications og Roles til databasen
                await _dbContext.SaveChangesAsync(cancellationToken);


                // Opret Respondent-objekter for hver email i request.RespondentEmails og tilføj dem til databasen
                var respondents = new List<Respondent>();

                foreach (var email in request.RespondentEmails)
                {
                    var respondent = new Respondent
                    {
                        RespondentId = Guid.NewGuid(),
                        EmailAddress = email,
                        HasAnswered = false,
                        AssessmentId = assessment.AssessmentId,
                        AccessToken = GenerateAccessToken()
                    };

                    respondents.Add(respondent);
                    _dbContext.Respondents.Add(respondent);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                // Mapping af Respondent-objekter til RespondentEmailContract-objekter, som bruges i AssessmentCreated-eventen.
                var respondentContracts = respondents
                 .Select(r => r.Adapt<RespondentEmailContract>())
                 .ToList();

                //Fortæl resten af systemet, at en ny Assessment er blevet oprettet.
                //Dette gøres via RabbitMQ, som sender en besked til de services, der lytter på "email-scheduled" routing key.
                var @event = new AssessmentCreated(
                    assessment.AssessmentId,
                    request.ApplicationName,
                    request.StartDate,
                    request.EndDate,
                    respondentContracts
                );

                await _publisher.PublishAsync(
                    @event,
                    "assessment-created",
                    cancellationToken);

                // Returner ID på den nye Assessment
                return assessment.AssessmentId;
            }
        }

        // Carter bruges til at definere Minimal API endpoints uden traditionelle Controllers.
        public class CreateAssessmentEndpoint : ICarterModule
        {
           
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                //HTTP POST
                // request indeholder data fra HTTP-requesten. ISender er MediatR's interface til at sende Commands og Queries videre til deres Handler.
                app.MapPost(
                    "api/assessments",
                    async (CreateAssessmentRequest request, ISender sender) =>
                    {

                        //Mapping fra CreateAssessmentRequest til CreateAssessment.Command. Dette gør det muligt at holde API-modellen adskilt fra Command-modellen.
                        //Mapster gør denne mapping automatisk ud fra properties med samme navn.
                        var command = request.Adapt<CreateAssessment.Command>();


                        // Sender Command'en til MediatR, som finder den korrekte Handler og kalder Handle() metoden.
                        var result = await sender.Send(command);

                        if (result.IsFailure)
                        {
                            return Results.BadRequest(result.Error);
                        }

                        return Results.Ok(result.Value);
                    });
            }
        }

    }
}
