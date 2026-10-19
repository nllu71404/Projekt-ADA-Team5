using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Shared;

namespace ADAProjectAPIVerticalSlice.Features.Authentication.Login
{
    public class Login
    {
        // Command indeholder de oplysninger, brugeren sender ved login.
        public class Command : IRequest<Result<string>>
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        // Validerer input, før handleren forsøger at logge brugeren ind.
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Email)
                    .NotEmpty()
                    .EmailAddress()
                    .WithMessage("Indtast en gyldig e-mailadresse.");

                RuleFor(c => c.Password)
                    .NotEmpty()
                    .WithMessage("Adgangskoden må ikke være tom.");
            }
        }


            public class Handler : IRequestHandler<Command, Result<string>>
            {
                private readonly UserManager<User> _userManager;
                private readonly IJwtTokenService _jwtTokenService;
                private readonly IValidator<Command> _validator;

                public Handler(
                    UserManager<User> userManager,
                    IJwtTokenService jwtTokenService,
                    IValidator<Command> validator)
                {
                    _userManager = userManager;
                    _jwtTokenService = jwtTokenService;
                    _validator = validator;
                }

                public async Task<Result<string>> Handle(
                    Command request,
                    CancellationToken cancellationToken)
                {
                    var validationResult = await _validator.ValidateAsync(
                        request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return Result.Failure<string>(
                            new Error(
                                "Login.Validation",
                                validationResult.ToString()));
                    }

                    var user = await _userManager.FindByEmailAsync(request.Email);

                    if (user is null)
                    {
                        return Result.Failure<string>(
                            new Error(
                                "Login.InvalidCredentials",
                                "E-mail eller adgangskode er forkert."));
                    }

                    var passwordIsValid = await _userManager.CheckPasswordAsync(
                        user, request.Password);

                    if (!passwordIsValid)
                    {
                        return Result.Failure<string>(
                            new Error(
                                "Login.InvalidCredentials",
                                "E-mail eller adgangskode er forkert."));
                    }

                    var token = await _jwtTokenService.CreateTokenAsync(user);

                    return Result.Success<string>(token);
                }
            }

            public class LoginEndpoint : ICarterModule
            {
                public void AddRoutes(IEndpointRouteBuilder app)
                {
                    app.MapPost(
                        "/api/auth/login",
                        async (
                            Command request,
                            ISender sender,
                            CancellationToken cancellationToken) =>
                        {
                            var result = await sender.Send(
                                request, cancellationToken);

                            if (result.IsFailure)
                            {
                                if (result.Error.Code == "Login.InvalidCredentials")
                                {
                                    return Results.Json(
                                        new { error = result.Error.Message },
                                        statusCode: StatusCodes.Status401Unauthorized);
                                }

                                if (result.Error.Code == "Login.Validation")
                                {
                                    return Results.BadRequest(new
                                    {
                                        error = result.Error.Message
                                    });
                                }

                                return Results.BadRequest(result.Error);
                            }

                            return Results.Ok(new
                            {
                                accessToken = result.Value
                            });
                        })
                        .AllowAnonymous()
                        .WithTags("Authentication");
                }
            }
        }
    }

