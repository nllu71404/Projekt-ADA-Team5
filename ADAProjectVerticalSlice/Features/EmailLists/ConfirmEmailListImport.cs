using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ADAProjectAPIVerticalSlice.Features.EmailLists;

public static class ConfirmEmailListImport
{
    public sealed record Response(
        int ImportedCount,
        int ExistingCount,
        int DuplicateCount,
        Guid? ImportBatchId);

    public sealed record Command(ValidateEmailListImport.Request Input)
        : IRequest<Result<Response>>;

    public sealed class Handler(
        ApplicationDbContext db,
        ICurrentUserService currentUser)
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Command command, CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Fail("Unauthorized", "Log ind for at importere kontakter.");
            }

            // Firmaet findes via den loggede bruger. Det kommer ikke fra CSV-filen.
            var companyId = await db.Users
                .Where(user => user.Id == userId)
                .Select(user => (Guid?)user.CompanyId)
                .SingleOrDefaultAsync(cancellationToken);

            if (companyId is not Guid id)
            {
                return Fail("Unauthorized", "Log ind for at importere kontakter.");
            }

            // Valider hele filen igen, før der bliver skrevet til databasen.
            var result = ValidateEmailListImport.Handler.Read(
                command.Input, cancellationToken);

            if (result.IsFailure)
            {
                return Result.Failure<Response>(result.Error);
            }

            var import = result.Value;
            if (import.Report.InvalidCount > 0)
            {
                return Fail("InvalidRows", "Ret de ugyldige poster, og valider filen igen.");
            }

            var emailKeys = import.Contacts
                .Select(contact => contact.NormalizedEmail)
                .ToArray();

            var existingKeys = await db.PeopleContacts
                .Where(contact => contact.CompanyId == id &&
                    emailKeys.Contains(contact.NormalizedEmail))
                .Select(contact => contact.NormalizedEmail)
                .ToListAsync(cancellationToken);

            var existing = existingKeys.ToHashSet(StringComparer.Ordinal);
            var batchId = Guid.NewGuid();
            var importedAt = DateTime.UtcNow;

            // Spring eksisterende kontakter over, så deres oplysninger bevares.
            var newContacts = import.Contacts
                .Where(contact => !existing.Contains(contact.NormalizedEmail))
                .Select(contact => new PeopleContact
                {
                    PeopleContactId = Guid.NewGuid(),
                    CompanyId = id,
                    Email = contact.Email,
                    NormalizedEmail = contact.NormalizedEmail,
                    FullName = contact.FullName,
                    JobTitle = contact.JobTitle,
                    Department = contact.Department,
                    Role = contact.Role,
                    Region = contact.Region,
                    Office = contact.Office,
                    YearsOfExperience = contact.YearsOfExperience,
                    ExperienceLevel = contact.ExperienceLevel,
                    ImportBatchId = batchId,
                    ImportedAtUtc = importedAt
                })
                .ToList();

            if (newContacts.Count > 0)
            {
                db.PeopleContacts.AddRange(newContacts);

                try
                {
                    // Gem de nye kontakter samlet, så en konflikt afbryder hele gemningen.
                    await db.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException exception) when (
                    exception.InnerException is PostgresException
                    {
                        SqlState: PostgresErrorCodes.UniqueViolation,
                        ConstraintName: "UX_PeopleContacts_CompanyId_NormalizedEmail"
                    })
                {
                    return Fail("ConcurrentImport",
                        "En anden import gemte en af adresserne. Prøv importen igen.");
                }
            }

            return Result.Success(new Response(
                newContacts.Count,
                existing.Count,
                import.Report.DuplicateCount,
                newContacts.Count == 0 ? null : batchId));
        }

        private static Result<Response> Fail(string code, string message)
        {
            return Result.Failure<Response>(
                new Error($"ConfirmEmailListImport.{code}", message));
        }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/api/email-lists/import/confirm",
                async (
                    ValidateEmailListImport.Request request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new Command(request), cancellationToken);
                    if (result.IsSuccess)
                    {
                        return Results.Ok(result.Value);
                    }

                    return result.Error.Code switch
                    {
                        "ConfirmEmailListImport.Unauthorized" => Results.Unauthorized(),
                        "ConfirmEmailListImport.ConcurrentImport" => Results.Conflict(result.Error),
                        _ => Results.BadRequest(result.Error)
                    };
                })
                .RequireAuthorization("AdminOnly")
                .WithTags("Email Lists");
        }
    }
}