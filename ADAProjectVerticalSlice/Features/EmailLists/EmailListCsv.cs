using ADAProjectAPIVerticalSlice.Shared;
using Microsoft.VisualBasic.FileIO;

namespace ADAProjectAPIVerticalSlice.Features.EmailLists;

internal static class EmailListCsv
{
    private const int MaxCsvCharacters = 250_000;

    public static Result<TextFieldParser> Open(string? csvText, string? delimiter)
    {
        if (string.IsNullOrWhiteSpace(csvText))
        {
            return Reject("Vælg en CSV-fil med kolonneoverskrifter.");
        }

        if (csvText.Length > MaxCsvCharacters)
        {
            return Reject("CSV-filen er for stor.");
        }

        delimiter ??= ",";
        if (delimiter != "," && delimiter != ";")
        {
            return Reject("Vælg komma eller semikolon som skilletegn.");
        }

        // Forhåndsvisning og validering skal læse den samme fil på samme måde.
        var csv = new TextFieldParser(
            new StringReader(csvText.TrimStart('\uFEFF')));

        csv.SetDelimiters(delimiter);
        csv.HasFieldsEnclosedInQuotes = true;

        return Result.Success(csv);
    }

    private static Result<TextFieldParser> Reject(string message)
    {
        return Result.Failure<TextFieldParser>(
            new Error("EmailListCsv.InvalidCsv", message));
    }
}