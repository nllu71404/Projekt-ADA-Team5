using Carter;
using MediatR;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ADAProjectAPIVerticalSlice.Shared;
using ADAProjectAPIVerticalSlice.Database;
using ADAProjectAPIVerticalSlice.Entities;

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
            }
        }

        // HANDLER = Handleren indeholder den konkrete logik for Command'en.
        // Denne Handler håndterer CreateAssessment.Command og returnerer et Result<Guid>.
        internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDbContext _dbContext;

            // Validatoren bliver injected via Dependency Injection. Den bruges til at kontrollere, om Command'en er gyldig.
            private readonly IValidator<Command> _validator;

            public Handler(
                ApplicationDbContext dbContext,
                IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
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

                var currentUserId = "1f1fb844-6fa0-4270-b5f5-56acca0fcb79"; // Placeholder, skal erstattes med den faktiske bruger-ID, når vi får sat JWT token eller session op.

                // Hvis valideringen lykkes, opretter vi en Assessment entity. Her går vi fra vores Command-model til den model, der repræsenterer data, som skal gemmes i databasen.
                var assessment = new Assessment
                {
                    AssessmentId = Guid.NewGuid(),
                    AssessmentName = request.AssessmentName,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,

                    ApplicationId = application.ApplicationId,

                    //Skal komme fra den autentificerede bruger, som sender requesten. 
                    //Dette kræver, at vi har en mekanisme til at hente den aktuelle bruger fra konteksten (f.eks. via JWT token eller session).
                    UserId = currentUserId 
                };

                _dbContext.Add(assessment);

                // EF Core oversætter operationen til SQL, som derefter sendes til SQL Server.
                await _dbContext.SaveChangesAsync(cancellationToken);

               // Fordi Handleren returnerer Result<Guid>, bliver Guid'en automatisk pakket ind som et Success Result.
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
