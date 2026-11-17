using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ADAProjectAPIVerticalSlice.Features.Respondents.ExchangeMagicLink
{
    public class ExchangeMagicLink
    {
        // Command indeholder magic link-tokenet fra respondentens e-mail.
        public class Command : IRequest<Result<ClaimsPrincipal>>
        {
            public string Token { get; set; } = string.Empty;
        }

        // Validerer input, før handleren forsøger at finde respondenten.
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Token)
                    .NotEmpty()
                    .WithMessage("Magic link-tokenet må ikke være tomt.");
            }
        }

        public class Handler : IRequestHandler<Command, Result<ClaimsPrincipal>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(
                ApplicationDbContext dbContext,
                IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<Result<ClaimsPrincipal>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                // 1. Validerer input.
                var validationResult = await _validator.ValidateAsync(
                    request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Result.Failure<ClaimsPrincipal>(
                        new Error(
                            "ExchangeMagicLink.Validation",
                            validationResult.ToString()));
                }

                // 2. Hasher det modtagne token, så det kan sammenlignes
                // med hashværdien, der er gemt i databasen.
                var tokenBytes = Encoding.UTF8.GetBytes(request.Token);
                var hashBytes = SHA256.HashData(tokenBytes);
                var tokenHash = Convert.ToHexString(hashBytes);

                // 3. Finder respondenten og den tilknyttede assessment.
                var respondent = await _dbContext.Respondents
                    .Include(r => r.Assessment)
                    .SingleOrDefaultAsync(
                        r => r.AccessToken == tokenHash,
                        cancellationToken);

                if (respondent is null)
                {
                    return Result.Failure<ClaimsPrincipal>(
                        new Error(
                            "ExchangeMagicLink.InvalidToken",
                            "Magic linket er ugyldigt."));
                }

                // 4. Kontrollerer, at respondenten ikke allerede har besvaret.
                if (respondent.HasAnswered)
                {
                    return Result.Failure<ClaimsPrincipal>(
                        new Error(
                            "ExchangeMagicLink.AlreadyAnswered",
                            "Surveyen er allerede besvaret."));
                }

                // 5. Kontrollerer, at assessmenten er åben.
                var now = DateTime.UtcNow;
                var assessment = respondent.Assessment;

                if (now < assessment.StartDate ||
                    now > assessment.EndDate)
                {
                    return Result.Failure<ClaimsPrincipal>(
                        new Error(
                            "ExchangeMagicLink.SurveyNotOpen",
                            "Surveyen er ikke åbent for besvarelse."));
                }

                // 6. Opretter respondentens claims.
                var claims = new List<Claim>
                {
                    new(
                        ClaimTypes.NameIdentifier,
                        respondent.RespondentId.ToString()),

                    new(
                        ClaimTypes.Email,
                        respondent.EmailAddress)
                };

                var identity = new ClaimsIdentity(
                    claims,
                    "RespondentCookie");

                var principal = new ClaimsPrincipal(identity);

                return Result.Success<ClaimsPrincipal>(principal);
            }
        }

        public class ExchangeMagicLinkEndpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost(
                    "/api/respondents/exchange-magic-link",
                    async (
                        Command request,
                        ISender sender,
                        HttpContext httpContext,
                        CancellationToken cancellationToken) =>
                    {
                        var result = await sender.Send(
                            request, cancellationToken);

                        if (result.IsFailure)
                        {
                            if (result.Error.Code == "ExchangeMagicLink.Validation")
                            {
                                return Results.BadRequest(new
                                {
                                    error = result.Error.Message
                                });
                            }

                            if (result.Error.Code == "ExchangeMagicLink.InvalidToken")
                            {
                                return Results.Json(
                                    new { error = result.Error.Message },
                                    statusCode: StatusCodes.Status401Unauthorized);
                            }

                            if (result.Error.Code == "ExchangeMagicLink.AlreadyAnswered" ||
                                result.Error.Code == "ExchangeMagicLink.SurveyNotOpen")
                            {
                                return Results.BadRequest(new
                                {
                                    error = result.Error.Message
                                });
                            }

                            return Results.BadRequest(result.Error);
                        }

                        // Begrænser sessionens levetid til assessmentens slutdato.
                        // RespondentTicketStore håndhæver også denne grænse.
                        var properties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            AllowRefresh = true
                        };

                        await httpContext.SignInAsync(
                            "RespondentCookie",
                            result.Value!,
                            properties);

                        return Results.Ok(new
                        {
                            message = "Magic link login successful"
                        });
                    })
                    .AllowAnonymous()
                    .WithTags("Respondents");
            }
        }
    }
}
