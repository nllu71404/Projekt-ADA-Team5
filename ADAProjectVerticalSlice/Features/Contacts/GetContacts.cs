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
        List<string> Offices,
        List<string> Departments,
        int Page,
        int PageCount,
        int TotalContacts);

    public sealed record Query(string? Office, string? Department, int? Page)
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

            var offices = await companyContacts
                .Where(contact => contact.Office != null && contact.Office != "")
                .Select(contact => contact.Office!)
                .Distinct()
                .OrderBy(office => office)
                .ToListAsync(cancellationToken);

            var departments = await companyContacts
                .Where(contact => contact.Department != null && contact.Department != "")
                .Select(contact => contact.Department!)
                .Distinct()
                .OrderBy(department => department)
                .ToListAsync(cancellationToken);

            // Når begge filtre er valgt, skal kontakten opfylde dem begge.
            var filtered = companyContacts;

            if (!string.IsNullOrWhiteSpace(request.Office))
            {
                filtered = filtered.Where(contact => contact.Office == request.Office);
            }

            if (!string.IsNullOrWhiteSpace(request.Department))
            {
                filtered = filtered.Where(contact => contact.Department == request.Department);
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
                contacts, offices, departments, page, pageCount, total));
        }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/api/contacts",
                async (
                    string? office,
                    string? department,
                    int? page,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new Query(office, department, page), cancellationToken);

                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.Unauthorized();
                })
                .RequireAuthorization("AdminOnly")
                .WithTags("Contacts");
        }
    }
}