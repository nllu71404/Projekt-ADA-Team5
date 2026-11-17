using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.Contacts;

public static class GetContacts
{
    public sealed record ContactRow(
    string? FullName,
    string Email,
    string? JobTitle,
    string? Office,
    string? Department,
    string? Role,
    string? Region,
    decimal? YearsOfExperience);

    public sealed record Response(
        List<ContactRow> Contacts,
        List<string> Roles,
        List<string> Regions,
        List<decimal> ExperienceYears,
        int Page,
        int PageCount,
        int TotalContacts);

    public sealed record Query(
        string? Role,
        string? Region,
        decimal? YearsOfExperience,
        int? Page)
        : IRequest<Result<Response>>;

    public sealed class Handler(
        ApplicationDbContext db,
        ICurrentUserService currentUser)
        : IRequestHandler<Query, Result<Response>>
    {
        private const int PageSize = 50;

        public async Task<Result<Response>> Handle(
            Query request, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;
            var companyId = await db.Users
                .Where(user => user.Id == userId)
                .Select(user => (Guid?)user.CompanyId)
                .SingleOrDefaultAsync(cancellationToken);

            if (companyId is not Guid id)
            {
                return Result.Failure<Response>(new Error(
                    "GetContacts.Unauthorized",
                    "Log ind for at se kontaktlisten."));
            }

            // Både kontakter og filtermuligheder skal tilhøre brugerens firma.
            var companyContacts = db.PeopleContacts
                .AsNoTracking()
                .Where(contact => contact.CompanyId == id);

            // Hent filtermuligheder fra hele firmaets kontaktliste før filtrering og sideinddeling.
            var roles = await companyContacts
                .Where(contact => contact.Role != null && contact.Role != "")
                .Select(contact => contact.Role!)
                .Distinct()
                .OrderBy(role => role)
                .ToListAsync(cancellationToken);

            var regions = await companyContacts
                .Where(contact => contact.Region != null && contact.Region != "")
                .Select(contact => contact.Region!)
                .Distinct()
                .OrderBy(region => region)
                .ToListAsync(cancellationToken);

            var experienceYears = await companyContacts
                .Where(contact => contact.YearsOfExperience.HasValue)
                .Select(contact => contact.YearsOfExperience.GetValueOrDefault())
                .Distinct()
                .OrderBy(years => years)
                .ToListAsync(cancellationToken);

            // Kontakten skal opfylde alle de valgte filtre.
            var filtered = companyContacts;

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                filtered = filtered.Where(contact => contact.Role == request.Role);
            }

            if (!string.IsNullOrWhiteSpace(request.Region))
            {
                filtered = filtered.Where(contact => contact.Region == request.Region);
            }

            if (request.YearsOfExperience is decimal yearsOfExperience)
            {
                filtered = filtered.Where(contact =>
                    contact.YearsOfExperience == yearsOfExperience);
            }

            var total = await filtered.CountAsync(cancellationToken);
            var pageCount = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            var page = Math.Clamp(request.Page ?? 1, 1, pageCount);

            var contacts = await filtered
                .OrderBy(contact => contact.FullName)
                .ThenBy(contact => contact.NormalizedEmail)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(contact => new ContactRow(
                    contact.FullName,
                    contact.Email,
                    contact.JobTitle,
                    contact.Office,
                    contact.Department,
                    contact.Role,
                    contact.Region,
                    contact.YearsOfExperience)) 
                .ToListAsync(cancellationToken);

            return Result.Success(new Response(
                contacts, roles, regions, experienceYears, page, pageCount, total));
        }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/api/contacts",
                async (
                        string? role,
                        string? region,
                        decimal? yearsOfExperience,
                        int? page,
                        ISender sender,
                        CancellationToken cancellationToken) =>
                    {
                    var result = await sender.Send(
                        new Query(role, region, yearsOfExperience, page), cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.Unauthorized();
                })
                .RequireAuthorization("AdminOnly")
                .WithTags("Contacts");
        }
    }
}