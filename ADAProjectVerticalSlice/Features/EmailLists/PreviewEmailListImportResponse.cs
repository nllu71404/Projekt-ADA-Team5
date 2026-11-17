namespace ADAProjectAPIVerticalSlice.Features.EmailLists;

public sealed record PreviewEmailListImportResponse(IReadOnlyList<PreviewEmailListImportResponse.Column> Columns, IReadOnlyList<string[]> Rows, bool HasMoreRows)
{
    public sealed record Column(int Index, string Label);
}