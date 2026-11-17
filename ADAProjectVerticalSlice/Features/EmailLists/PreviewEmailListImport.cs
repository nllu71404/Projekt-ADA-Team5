using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.VisualBasic.FileIO;

namespace ADAProjectAPIVerticalSlice.Features.EmailLists;

public static class PreviewEmailListImport
{
    public sealed record Command(string? CsvText, string? Delimiter)
        : IRequest<Result<PreviewEmailListImportResponse>>;

    public sealed class Handler
        : IRequestHandler<Command, Result<PreviewEmailListImportResponse>>
    {
        private const int MaxPreviewRows = 10;

        public Task<Result<PreviewEmailListImportResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(BuildPreview(request, cancellationToken));
        }

        private static Result<PreviewEmailListImportResponse> BuildPreview(
            Command request,
            CancellationToken cancellationToken)
        {
            var input = EmailListCsv.Open(request.CsvText, request.Delimiter);
            if (input.IsFailure)
            {
                return Result.Failure<PreviewEmailListImportResponse>(input.Error);
            }

            using var csv = input.Value;

            try
            {
                var headers = csv.ReadFields();
                if (headers is null || headers.All(string.IsNullOrWhiteSpace))
                {
                    return InvalidCsv("CSV-filen mangler kolonneoverskrifter.");
                }

                // Vi bruger kolonnens placering ved valg af e-mailfelt,
                // så forskellige eller ens kolonnenavne kan håndteres.
                var columns = headers
                    .Select((header, index) => new PreviewEmailListImportResponse.Column(
                        index,
                        string.IsNullOrWhiteSpace(header)
                            ? $"Kolonne {index + 1}"
                            : header.Trim()))
                    .ToArray();

                var rows = new List<string[]>();

                // Forhåndsvisningen viser kun de første rækker. Hele filen
                // skal valideres, når brugeren senere bekræfter importen.
                while (rows.Count < MaxPreviewRows && !csv.EndOfData)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fields = csv.ReadFields();
                    if (fields is null)
                    {
                        break;
                    }

                    if (fields.Length != columns.Length)
                    {
                        return InvalidCsv(
                            "En række har et andet antal felter end overskriften. "
                            + "Kontrollér skilletegnet og filens opbygning.");
                    }

                    rows.Add(fields);
                }

                return Result.Success(new PreviewEmailListImportResponse(
                    columns,
                    rows,
                    !csv.EndOfData));
            }
            catch (MalformedLineException)
            {
                return InvalidCsv(
                    $"CSV-filen kunne ikke læses omkring linje {csv.ErrorLineNumber}.");
            }
        }

        private static Result<PreviewEmailListImportResponse> InvalidCsv(string message)
        {
            return Result.Failure<PreviewEmailListImportResponse>(
                new Error("PreviewEmailListImport.InvalidCsv", message));
        }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/api/email-lists/import/preview",
                async (
                    PreviewEmailListImportRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new Command(request.CsvText, request.Delimiter);
                    var result = await sender.Send(command, cancellationToken);

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