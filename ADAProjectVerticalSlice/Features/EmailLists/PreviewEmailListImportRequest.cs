namespace ADAProjectAPIVerticalSlice.Features.EmailLists;

public sealed record PreviewEmailListImportRequest(string? CsvText, string? Delimiter);