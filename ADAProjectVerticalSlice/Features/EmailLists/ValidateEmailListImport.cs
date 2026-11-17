using System.Net.Mail;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;

namespace ADAProjectAPIVerticalSlice.Features.EmailLists;

public static class ValidateEmailListImport
{
    public sealed record Request(
    string? CsvText,
    string? Delimiter,
    int? EmailColumnIndex,
    ColumnMapping? OptionalColumns = null);

    public sealed record ColumnMapping(
        int? FullName,
        int? JobTitle,
        int? Department,
        int? Role,
        int? Region,
        int? YearsOfExperience,
        int? ExperienceLevel,
        int? Office = null);

    public sealed record RowIssue(
        int RecordNumber, string? Email, string Kind, string Message);

    public sealed record Response(
        int RowsChecked, int ValidCount, int InvalidCount, int DuplicateCount,
        List<RowIssue> Issues);

    internal sealed record ValidatedContact(
    string Email,
    string NormalizedEmail,
    string? FullName,
    string? JobTitle,
    string? Department,
    string? Role,
    string? Region,
    string? Office,
    decimal? YearsOfExperience,
    string? ExperienceLevel);

    internal sealed record ValidatedImport(
        Response Report,
        List<ValidatedContact> Contacts);

    public sealed record Command(Request Input) : IRequest<Result<Response>>;

    public sealed class Handler : IRequestHandler<Command, Result<Response>>
    {
        private const int MaxDataRecords = 2_000;

        public Task<Result<Response>> Handle(
        Command command, CancellationToken cancellationToken)
        {
            var result = Read(command.Input, cancellationToken);

            return Task.FromResult(result.IsSuccess
                ? Result.Success(result.Value.Report)
                : Result.Failure<Response>(result.Error));
        }

        // Validering og bekræftelse bruger de samme regler for CSV-filen.
        internal static Result<ValidatedImport> Read(
            Request request, CancellationToken cancellationToken)
        {
            var input = EmailListCsv.Open(request.CsvText, request.Delimiter);
            if (input.IsFailure)
            {
                return Result.Failure<ValidatedImport>(input.Error);
            }

            using var csv = input.Value;

            try
            {
                var headers = csv.ReadFields();
                if (headers is null || headers.All(string.IsNullOrWhiteSpace))
                {
                    return Reject("CSV-filen mangler kolonneoverskrifter.");
                }

                if (request.EmailColumnIndex is not int emailIndex ||
                    emailIndex < 0 || emailIndex >= headers.Length)
                {
                    return Reject("Vælg en e-mailkolonne fra forhåndsvisningen.");
                }

                var mapping = request.OptionalColumns;

                // Valider de valgte kolonneindeks, før vi læser værdierne fra rækkerne.
                var optionalFields = new (string Label, int? Index, bool IsYears)[]
                {
    ("Navn", mapping?.FullName, false),
    ("Jobtitel", mapping?.JobTitle, false),
    ("Afdeling", mapping?.Department, false),
    ("Rolle", mapping?.Role, false),
    ("Region", mapping?.Region, false),
    // Kontor er et arbejdssted. Region er et selvstændigt felt.
    ("Kontor", mapping?.Office, false),
    ("Års erfaring", mapping?.YearsOfExperience, true),
    ("Erfaringsniveau", mapping?.ExperienceLevel, false)
                };

                var selectedIndices = new HashSet<int> { emailIndex };

                foreach (var field in optionalFields)
                {
                    if (field.Index is not int index)
                    {
                        continue;
                    }

                    if (index < 0 || index >= headers.Length)
                    {
                        return Reject($"{field.Label} skal pege på en kolonne i CSV-filen.");
                    }

                    if (!selectedIndices.Add(index))
                    {
                        return Reject("En CSV-kolonne kan kun bruges til ét felt.");
                    }
                }

                // Første forekomst beholdes. Store og små bogstaver behandles ens.
                // Brug samme nøgle til dubletter i filen og i firmaets kontaktliste.
                var firstOccurrence = new Dictionary<string, int>(StringComparer.Ordinal);
                var contacts = new List<ValidatedContact>();
                var issues = new List<RowIssue>();
                var rowsChecked = 0;
                var duplicateCount = 0;

                while (!csv.EndOfData)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fields = csv.ReadFields();
                    if (fields is null)
                    {
                        break;
                    }

                    if (++rowsChecked > MaxDataRecords)
                    {
                        return Reject("Opdel CSV-filen i filer med højst 2.000 datarækker.");
                    }

                    // Overskriften tæller som post 1. En post kan fylde flere tekstlinjer.
                    var recordNumber = rowsChecked + 1;

                    if (fields.Length != headers.Length)
                    {
                        issues.Add(new RowIssue(recordNumber, null, "invalid",
                            "Antallet af felter passer ikke til overskriften."));
                        continue;
                    }

                    var email = fields[emailIndex].Trim();

                    if (email.Length == 0)
                    {
                        issues.Add(new RowIssue(recordNumber, null, "invalid",
                            "E-mailadressen mangler."));
                        continue;
                    }

                    // Feltet skal indeholde en adresse uden visningsnavn.
                    if (email.Length > 254 ||
                        !MailAddress.TryCreate(email, out var address) ||
                        !string.Equals(address.Address, email, StringComparison.Ordinal))
                    {
                        issues.Add(new RowIssue(recordNumber, email, "invalid",
                            "Indtast én gyldig e-mailadresse uden visningsnavn."));
                        continue;
                    }

                    decimal? yearsOfExperience = null;
                    var issueCountBeforeOptionalFields = issues.Count;

                    foreach (var field in optionalFields)
                    {
                        if (field.Index is not int index)
                        {
                            continue;
                        }

                        var value = fields[index].Trim();

                        // Et tomt valgfrit felt er gyldigt, også når kolonnen er valgt.
                        if (value.Length == 0)
                        {
                            continue;
                        }

                        if (field.IsYears)
                        {
                            // Accepter både dansk decimalkomma og decimalpunktum.
                            var normalized = value.Replace(',', '.');
                            var isValid = decimal.TryParse(
                                normalized,
                                NumberStyles.AllowDecimalPoint,
                                CultureInfo.InvariantCulture,
                                out var years);

                            if (!isValid || years < 0)
                            {
                                issues.Add(new RowIssue(
                                    recordNumber, email, "invalid",
                                    $"{field.Label} skal være et tal på 0 eller derover, fx 2 eller 2,5."));
                            }
                            else
                            {
                                // Gem den fortolkede værdi, så f.eks. 2,5 bliver gemt som et tal.
                                yearsOfExperience = years;
                            }

                        }
                        else if (value.Length > 120)
                        {
                            issues.Add(new RowIssue(
                                recordNumber, email, "invalid",
                                $"{field.Label} må højst indeholde 120 tegn."));
                        }
                    }

                    // Kun gyldige poster kan være den første forekomst af en adresse.
                    if (issues.Count > issueCountBeforeOptionalFields)
                    {
                        continue;
                    }

                    var normalizedEmail = email.ToUpperInvariant();

                    if (!firstOccurrence.TryAdd(normalizedEmail, recordNumber))
                    {
                        duplicateCount++;
                        issues.Add(new RowIssue(
                            recordNumber, email, "duplicate",
                            $"Adressen findes allerede i CSV-post {firstOccurrence[normalizedEmail]}."));
                        continue;
                    }

                    // Kun validerede værdier fra de valgte kolonner går videre til gemning.
                    contacts.Add(new ValidatedContact(
                        email,
                        normalizedEmail,
                        ReadOptional(fields, mapping?.FullName),
                        ReadOptional(fields, mapping?.JobTitle),
                        ReadOptional(fields, mapping?.Department),
                        ReadOptional(fields, mapping?.Role),
                        ReadOptional(fields, mapping?.Region),
                        ReadOptional(fields, mapping?.Office),
                        yearsOfExperience,
                        ReadOptional(fields, mapping?.ExperienceLevel)));
                }

                if (rowsChecked == 0)
                {
                    return Reject("CSV-filen indeholder ingen kontaktposter.");
                }

                var report = new Response(
                    rowsChecked,
                    contacts.Count,
                    issues.Where(issue => issue.Kind == "invalid")
                        .Select(issue => issue.RecordNumber)
                        .Distinct()
                        .Count(),
                    duplicateCount,
                    issues);

                return Result.Success(new ValidatedImport(report, contacts));
            }
            catch (MalformedLineException)
            {
                return Reject(
                    $"CSV-filen kunne ikke læses omkring linje {csv.ErrorLineNumber}.");
            }
        }

        private static string? ReadOptional(string[] fields, int? index)
        {
            var value = index is int column ? fields[column].Trim() : null;
            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static Result<ValidatedImport> Reject(string message)
        {
            return Result.Failure<ValidatedImport>(
                new Error("ValidateEmailListImport.InvalidCsv", message));
        }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/api/email-lists/import/validate",
                async (Request request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new Command(request), cancellationToken);

                    if (result.IsFailure)
                    {
                        return Results.BadRequest(result.Error);
                    }

                    return Results.Ok(result.Value);
                })
                .RequireAuthorization("AdminOnly")
                .WithTags("Email Lists");
        }
    }
}