using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Database.Auditing
{
    // Udfører databaseoperationer med audit-oplysninger.
    public sealed class AuditDbExecutor(
        IServiceScopeFactory scopeFactory)
    {
        // Udfør en databaseoperation med bruger- og forespørgsels-ID.
        public async Task ExecuteAsync(
            string actorId,
            string requestId,
            Func<ApplicationDbContext, CancellationToken, Task> operation,
            CancellationToken ct = default)
        {
            // Opret et separat scope til databasens retry-strategi.
            await using var strategyScope =
                scopeFactory.CreateAsyncScope();

            // Hent databasekonteksten til retry-strategien.
            var strategyDb = strategyScope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            // Opret en strategi til håndtering af midlertidige databasefejl.
            var strategy =
                strategyDb.Database.CreateExecutionStrategy();

            // Udfør operationen med automatisk genforsøg ved midlertidige fejl.
            await strategy.ExecuteAsync(async () =>
            {
                // Opret et nyt scope for hvert forsøg.
                await using var scope =
                    scopeFactory.CreateAsyncScope();

                // Hent en ny databasekontekst.
                var db = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                // Start en databasetransaktion.
                await using var tx =
                    await db.Database.BeginTransactionAsync(ct);

                // Gem applikationsbrugerens ID i den aktuelle transaktion.
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT set_config('app.actor_id', {actorId}, true)",
                    ct);

                // Gem forespørgsels-ID i den aktuelle transaktion.
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT set_config('app.request_id', {requestId}, true)",
                    ct);

                // Udfør den angivne databaseoperation.
                await operation(db, ct);

                // Gem ændringerne i databasen.
                await db.SaveChangesAsync(ct);

                // Godkend transaktionen.
                await tx.CommitAsync(ct);
            });
        }
    }
}