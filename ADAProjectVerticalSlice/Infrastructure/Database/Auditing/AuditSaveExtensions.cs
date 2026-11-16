using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Database.Auditing;

public static class AuditSaveExtensions
{
    public static async Task SaveChangesWithAuditAsync(
        this ApplicationDbContext dbContext,
        string actorId,
        string? requestId,
        CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsRelational())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Oprettelse knytter målingen til brugeren. Rettelse og sletning
        // kræver ejerskab, så vi kan hente målingens firma via brugeren.
        var companyId = await dbContext.Users
            .Where(user => user.Id == actorId)
            .Select(user => user.CompanyId)
            .SingleAsync(cancellationToken);

        // Audittriggerne læser værdierne i den samme transaktion som gemningen.
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.actor_id', {actorId}, true)",
            cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.company_id', {companyId.ToString("D")}, true)",
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(requestId))
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT set_config('app.request_id', {requestId}, true)",
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}