using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using ADAProjectVerticalSlice.Features.Assessments.UpdateAssessment;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using ADAProjectAPIVerticalSlice.Infrastructure.Database.Auditing;

namespace ADAProjectAPIVerticalSlice.Features.Assessments.UpdateAssessment
{
    public class UpdateAssessment
    {
        // COMMAND
        public class Command : IRequest<Result<Guid>>
        {
            public Guid AssessmentId { get; set; }

            public string? RequestId { get; set; }

            public string AssessmentName { get; set; } = string.Empty;

            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }

            public string ApplicationName { get; set; } = string.Empty;

            public List<string> RoleNames { get; set; } = new();

            public List<Guid> RegionIds { get; set; } = new();

            public List<Guid> ExperienceIds { get; set; } = new();

            public List<string> RespondentEmails { get; set; } = new();
        }

        // VALIDATOR
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.AssessmentName)
                    .NotEmpty()
                    .WithMessage("Målingsnavnet må ikke være tomt.");

                RuleFor(c => c.EndDate)
                    .GreaterThan(c => c.StartDate)
                    .WithMessage("Slutdato skal være efter startdato.");

                RuleFor(c => c.ApplicationName)
                    .NotEmpty()
                    .WithMessage("Applikationsnavnet må ikke være tomt.");

                RuleFor(c => c.RoleNames)
                    .NotEmpty()
                    .WithMessage("Mindst én rolle skal være valgt.");

                RuleFor(c => c.RegionIds)
                    .NotEmpty()
                    .WithMessage("Mindst én region skal være valgt.");

                RuleFor(c => c.ExperienceIds)
                    .NotEmpty()
                    .WithMessage("Mindst ét erfaringsinterval skal være valgt.");

                RuleForEach(c => c.RespondentEmails)
                    .EmailAddress()
                    .WithMessage("En eller flere e-mailadresser er ugyldige.");
            }
        }

        // HANDLER
        public class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;
            private readonly ICurrentUserService _currentUserService;

            public Handler(
                ApplicationDbContext dbContext,
                IValidator<Command> validator,
                ICurrentUserService currentUserService)
            {
                _dbContext = dbContext;
                _validator = validator;
                _currentUserService = currentUserService;
            }

            // Henter de valgte regioner fra databasen.
            private async Task<Result<List<Region>>> GetRegions(
                List<Guid> regionIds,
                CancellationToken cancellationToken)
            {
                var uniqueIds = regionIds.Distinct().ToList();

                var regions = await _dbContext.Regions
                    .Where(r => uniqueIds.Contains(r.RegionId))
                    .ToListAsync(cancellationToken);

                if (regions.Count != uniqueIds.Count)
                {
                    return Result.Failure<List<Region>>(
                        new Error(
                            "UpdateAssessment.RegionNotFound",
                            "Regioner kunne ikke findes."));
                }

                return regions;
            }

            // Henter de valgte erfaringsniveauer fra databasen.
            private async Task<Result<List<Experience>>> GetExperiences(
                List<Guid> experienceIds,
                CancellationToken cancellationToken)
            {
                var uniqueIds = experienceIds.Distinct().ToList();

                var experiences = await _dbContext.Experiences
                    .Where(e => uniqueIds.Contains(e.ExperienceId))
                    .ToListAsync(cancellationToken);

                if (experiences.Count != uniqueIds.Count)
                {
                    return Result.Failure<List<Experience>>(
                        new Error(
                            "UpdateAssessment.ExperienceNotFound",
                            "Erfaringer kunne ikke findes."));
                }

                return experiences;
            }

            public async Task<Result<Guid>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                // 1. Valider requesten.
                var validationResult = await _validator.ValidateAsync(
                    request,
                    cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Result.Failure<Guid>(
                        new Error(
                            "UpdateAssessment.Validation",
                            validationResult.ToString()));
                }

                // 2. Hent den aktuelle bruger.
                var currentUserId = _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return Result.Failure<Guid>(Error.NullValue);
                }

                // 3. Hent vurderingen, men kun hvis den tilhører brugeren.
                var assessment = await _dbContext.Assessments
                    .Include(a => a.Respondents)
                    .Include(a => a.Roles)
                    .Include(a => a.Regions)
                    .Include(a => a.Experiences)
                    .FirstOrDefaultAsync(
                        a => a.AssessmentId == request.AssessmentId
                             && a.UserId == currentUserId,
                        cancellationToken);

                if (assessment is null)
                {
                    return Result.Failure<Guid>(
                        new Error(
                            "UpdateAssessment.NotFound",
                            "Vurderingen kunne ikke findes."));
                }

                // 4. Find eller opret Application.
                var application = await _dbContext.Applications
                    .FirstOrDefaultAsync(
                        a => a.ApplicationName == request.ApplicationName,
                        cancellationToken);

                if (application is null)
                {
                    application = new Application
                    {
                        ApplicationId = Guid.NewGuid(),
                        ApplicationName = request.ApplicationName
                    };

                    _dbContext.Applications.Add(application);
                }

                // 5. Find eller opret Roles.
                var roles = new List<Role>();

                var roleNames = request.RoleNames
                    .Select(name => name.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (var roleName in roleNames)
                {
                    var role = await _dbContext.Roles
                        .FirstOrDefaultAsync(
                            r => r.RoleName == roleName,
                            cancellationToken);

                    if (role is null)
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

                // 6. Hent de valgte Regions og Experiences.
                var regionsResult = await GetRegions(
                    request.RegionIds,
                    cancellationToken);

                if (regionsResult.IsFailure)
                {
                    return Result.Failure<Guid>(regionsResult.Error);
                }

                var experiencesResult = await GetExperiences(
                    request.ExperienceIds,
                    cancellationToken);

                if (experiencesResult.IsFailure)
                {
                    return Result.Failure<Guid>(experiencesResult.Error);
                }

                // 7. Opdater vurderingens almindelige felter.
                assessment.AssessmentName = request.AssessmentName;
                assessment.StartDate = request.StartDate;
                assessment.EndDate = request.EndDate;

                assessment.ApplicationId = application.ApplicationId;

                // 8. Opdater relationerne til Roles, Regions og Experiences.
                assessment.Roles.Clear();
                assessment.Roles.AddRange(roles);

                assessment.Regions.Clear();
                assessment.Regions.AddRange(regionsResult.Value);

                assessment.Experiences.Clear();
                assessment.Experiences.AddRange(experiencesResult.Value);

                // 9. Tilføj nye respondenter.
                // Eksisterende respondenter bliver ikke slettet.
                var existingEmails = assessment.Respondents
                    .Select(r => r.EmailAddress.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var newEmails = request.RespondentEmails
                    .Select(email => email.Trim())
                    .Where(email => !existingEmails.Contains(email))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (var email in newEmails)
                {
                    var respondent = new Respondent
                    {
                        RespondentId = Guid.NewGuid(),
                        EmailAddress = email,
                        HasAnswered = false,
                        AssessmentId = assessment.AssessmentId,
                        AccessToken = Convert.ToBase64String(
                            RandomNumberGenerator.GetBytes(32)),
                        RegionId = null,
                        ExperienceId = null,
                        RoleId = null
                    };

                    _dbContext.Respondents.Add(respondent);
                }

                if (_dbContext.Database.IsRelational())
                {
                    await _dbContext.SaveChangesWithAuditAsync(currentUserId, request.RequestId, cancellationToken);
                }
                else
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                return assessment.AssessmentId;
            }
        }

        // CARTER ENDPOINT
        public class UpdateAssessmentEndpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPut(
                    "api/assessments/{assessmentId:guid}",
                    async (
                        Guid assessmentId,
                        UpdateAssessmentRequest request,
                        ISender sender,
                        HttpContext httpContext) =>
                        {
                        var command = new Command
                        {
                            AssessmentId = assessmentId,
                            RequestId = httpContext.TraceIdentifier,
                            AssessmentName = request.AssessmentName,
                            StartDate = request.StartDate,
                            EndDate = request.EndDate,
                            ApplicationName = request.ApplicationName,
                            RoleNames = request.RoleNames,
                            RegionIds = request.RegionIds,
                            ExperienceIds = request.ExperienceIds,
                            RespondentEmails = request.RespondentEmails
                        };

                        var result = await sender.Send(command);

                        if (result.IsFailure)
                        {
                            return Results.BadRequest(result.Error);
                        }

                        return Results.Ok(result.Value);
                    })
                    .RequireAuthorization("AdminOnly");
            }
        }
    }
}
