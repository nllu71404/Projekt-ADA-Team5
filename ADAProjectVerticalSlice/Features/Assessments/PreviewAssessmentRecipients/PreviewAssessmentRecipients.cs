using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Assessments.PreviewAssessmentRecipients;

public static class PreviewAssessmentRecipients
{
    public sealed record Recipient(string? FullName, string Email);

    public sealed record Response(int TotalCount, List<Recipient> Recipients);

    public sealed record Query(PreviewAssessmentRecipientsRequest Request)
        : IRequest<Result<Response>>;

    public sealed class Handler : IRequestHandler<Query, Result<Response>>
    {
        private const int PreviewLimit = 50;

        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public Handler(
            ApplicationDbContext dbContext,
            ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Response>> Handle(
            Query query, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var companyId = await _dbContext.Users
                .Where(user => user.Id == userId)
                .Select(user => (Guid?)user.CompanyId)
                .SingleOrDefaultAsync(cancellationToken);

            if (companyId is not Guid id)
            {
                return Result.Failure<Response>(new Error(
                    "PreviewAssessmentRecipients.Unauthorized",
                    "Log ind for at se modtagerne."));
            }

            var regionIds = (query.Request.RegionIds ?? [])
                .Distinct()
                .ToArray();

            var roleNames = (query.Request.RoleNames ?? [])
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role!.Trim().ToUpperInvariant())
                .Distinct()
                .ToArray();

            if (regionIds.Length == 0 && roleNames.Length == 0)
            {
                return Result.Failure<Response>(new Error(
                    "PreviewAssessmentRecipients.MissingFilters",
                    "Vælg mindst én region eller angiv mindst én rolle."));
            }

            // Modtagere findes altid inden for den loggede brugers firma.
            var matchingContacts = _dbContext.PeopleContacts
                .AsNoTracking()
                .Where(contact => contact.CompanyId == id);

            // Uden valgte regioner kan kontakter fra alle regioner matche.
            if (regionIds.Length > 0)
            {
                var regionNames = await _dbContext.Regions
                    .Where(region => regionIds.Contains(region.RegionId))
                    .Select(region => region.RegionName.ToUpper())
                    .ToListAsync(cancellationToken);

                if (regionNames.Count != regionIds.Length)
                {
                    return Result.Failure<Response>(new Error(
                        "PreviewAssessmentRecipients.UnknownRegion",
                        "En eller flere valgte regioner findes ikke."));
                }

                matchingContacts = matchingContacts.Where(contact =>
                    contact.Region != null &&
                    regionNames.Contains(contact.Region.ToUpper()));
            }

            if (roleNames.Length > 0)
            {
                matchingContacts = matchingContacts.Where(contact =>
                    contact.Role != null &&
                    roleNames.Contains(contact.Role.ToUpper()));
            }

            var totalCount = await matchingContacts.CountAsync(cancellationToken);

            var recipients = await matchingContacts
                .OrderBy(contact => contact.FullName)
                .ThenBy(contact => contact.NormalizedEmail)
                .Take(PreviewLimit)
                .Select(contact => new Recipient(contact.FullName, contact.Email))
                .ToListAsync(cancellationToken);

            return Result.Success(new Response(totalCount, recipients));
        }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/api/assessments/recipients/preview",
                async (
                    PreviewAssessmentRecipientsRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new Query(request), cancellationToken);

                    if (result.IsSuccess)
                    {
                        return Results.Ok(result.Value);
                    }

                    if (result.Error.Code == "PreviewAssessmentRecipients.Unauthorized")
                    {
                        return Results.Unauthorized();
                    }

                    return Results.BadRequest(result.Error);
                })
                .RequireAuthorization("AdminOnly")
                .WithTags("Assessments");
        }
    }
}