using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Shared;

namespace ADAProjectAPIVerticalSlice.Features.Authentication.WebLogin
{
    public class WebLogin
    {
        // Command indeholder de oplysninger, brugeren sender ved web-login.
        public class Command : IRequest<Result<ClaimsPrincipal>>
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

        public class Handler : IRequestHandler<Command, Result<ClaimsPrincipal>>
        {
            private readonly UserManager<User> _userManager;
            private readonly SignInManager<User> _signInManager;
            private readonly IValidator<Command> _validator;

            public Handler(
                UserManager<User> userManager,
                SignInManager<User> signInManager,
                IValidator<Command> validator)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _validator = validator;
            }

            public async Task<Result<ClaimsPrincipal>> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(
                    request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Result.Failure<ClaimsPrincipal>(
                        new Error(
                            "WebLogin.Validation",
                            validationResult.ToString()));
                }

                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user is null)
                {
                    return Result.Failure<ClaimsPrincipal>(
                        new Error(
                            "WebLogin.InvalidCredentials",
                            "E-mail eller adgangskode er forkert."));
                }

                // Kontrollerer password og håndterer Identitys lockout-regler.
                var signInResult = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password,
                    lockoutOnFailure: true);

                if (!signInResult.Succeeded)
                {
                    return Result.Failure<ClaimsPrincipal>(
                        new Error(
                            "WebLogin.InvalidCredentials",
                            "E-mail eller adgangskode er forkert."));
                }

                // Opretter de samme claims som i JwtTokenService.
                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, user.Id),
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new("companyId", user.CompanyId.ToString())
                };

                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, user.Email));
                }

                if (!string.IsNullOrWhiteSpace(user.FullName))
                {
                    claims.Add(new Claim(ClaimTypes.Name, user.FullName));
                }

                var roles = await _userManager.GetRolesAsync(user);

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                return Result.Success<ClaimsPrincipal>(principal);
            }
        }

        public class WebLoginEndpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost(
                    "/api/auth/web-login",
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
                            if (result.Error.Code == "WebLogin.InvalidCredentials")
                            {
                                return Results.Json(
                                    new { error = result.Error.Message },
                                    statusCode: StatusCodes.Status401Unauthorized);
                            }

                            if (result.Error.Code == "WebLogin.Validation")
                            {
                                return Results.BadRequest(new
                                {
                                    error = result.Error.Message
                                });
                            }

                            return Results.BadRequest(result.Error);
                        }

                        var properties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            AllowRefresh = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                        };

                        await httpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            result.Value!,
                            properties);

                        return Results.Ok(new
                        {
                            message = "Login successful"
                        });
                    })
                    .AllowAnonymous()
                    .WithTags("Authentication");
            }
        }
    }
}
