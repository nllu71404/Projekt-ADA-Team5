using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Authentication
{
    public class PostgresTicketStore : ITicketStore
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDataProtector _protector;

        public PostgresTicketStore(
            IServiceScopeFactory scopeFactory,
            IDataProtectionProvider dataProtectionProvider)
        {
            _scopeFactory = scopeFactory;

            // Bruges til at beskytte authentication-ticketen.
            _protector = dataProtectionProvider.CreateProtector(
                "ADA.Authentication.CookieTicket.v1");
        }

        // Gemmer en ny authentication-ticket i PostgreSQL.
        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            // Henter brugerens ID fra authentication-ticketen.
            var userId = ticket.Principal
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException(
                    "Authentication-ticket mangler brugerens ID.");
            }

            // Opretter et tilfældigt session-ID.
            var id = new Guid(RandomNumberGenerator.GetBytes(16));

            // Serialiserer og beskytter ticketen før lagring.
            var serializedTicket = TicketSerializer.Default.Serialize(ticket);
            var protectedTicket = _protector.Protect(serializedTicket);

            var session = new UserSession
            {
                Id = id,
                UserId = userId,
                Ticket = protectedTicket,
                Created = now,
                LastActivity = now,
                Expires = GetExpiration(ticket, now)
            };

            dbContext.UserSessions.Add(session);

            await dbContext.SaveChangesAsync();

            // ID'et bliver den reference, Cookie Authentication bruger.
            return id.ToString("N");
        }

        // Henter en eksisterende authentication-ticket.
        public async Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            if (!Guid.TryParseExact(key, "N", out var id))
            {
                return null;
            }

            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            // Henter sessionen fra databasen.
            // Vi bruger ikke AsNoTracking, da vi skal opdatere LastActivity.
            var session = await dbContext.UserSessions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session is null)
            {
                return null;
            }

            // Afviser tilbagekaldte og udløbne sessioner.
            if (session.Revoked.HasValue ||
                session.Expires <= DateTime.UtcNow)
            {
                return null;
            }

            try
            {
                // Henter og dekrypterer authentication-ticketen.
                var serializedTicket = _protector.Unprotect(session.Ticket);

                var ticket = TicketSerializer.Default.Deserialize(serializedTicket);

                if (ticket is null)
                {
                    return null;
                }

                // Opdaterer tidspunktet for seneste aktivitet.
                session.LastActivity = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();

                return ticket;
            }
            catch (CryptographicException)
            {
                // Hvis ticketen er ugyldig, accepteres sessionen ikke.
                return null;
            }
        }

        // Fornyer en eksisterende session.
        public async Task RenewAsync(
            string key,
            AuthenticationTicket ticket)
        {
            if (!Guid.TryParseExact(key, "N", out var id))
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var session = await dbContext.UserSessions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session is null ||
                session.Revoked.HasValue ||
                session.Expires <= DateTime.UtcNow)
            {
                return;
            }

            var now = DateTime.UtcNow;

            var serializedTicket = TicketSerializer.Default.Serialize(ticket);

            session.Ticket = _protector.Protect(serializedTicket);
            session.LastActivity = now;
            session.Expires = GetExpiration(ticket, now);

            await dbContext.SaveChangesAsync();
        }

        // Tilbagekalder en session.
        public async Task RemoveAsync(string key)
        {
            if (!Guid.TryParseExact(key, "N", out var id))
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var session = await dbContext.UserSessions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session is null || session.Revoked.HasValue)
            {
                return;
            }

            // Vi beholder rækken i databasen som historik.
            session.Revoked = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
        }

        // Finder ticketens udløbstidspunkt.
        private static DateTime GetExpiration(
            AuthenticationTicket ticket,
            DateTime now)
        {
            return ticket.Properties.ExpiresUtc?.UtcDateTime
                ?? now.AddHours(8);
        }
    }
}
