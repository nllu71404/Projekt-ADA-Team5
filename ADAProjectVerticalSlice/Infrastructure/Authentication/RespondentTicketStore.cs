using ADAProjectAPIVerticalSlice.Entities;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ADAProjectAPIVerticalSlice.Infrastructure.Authentication
{
    public class RespondentTicketStore : ITicketStore
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDataProtector _protector;

        public RespondentTicketStore(
            IServiceScopeFactory scopeFactory,
            IDataProtectionProvider dataProtectionProvider)
        {
            _scopeFactory = scopeFactory;

            // Egen purpose til respondenternes authentication-tickets.
            _protector = dataProtectionProvider.CreateProtector(
                "ADA.Respondent.Authentication.CookieTicket.v1");
        }

        // Gemmer en ny respondent-session i PostgreSQL.
        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            // Henter respondentens ID fra authentication-ticketen.
            var respondentIdValue = ticket.Principal
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(respondentIdValue, out var respondentId))
            {
                throw new InvalidOperationException(
                    "Authentication-ticket mangler respondentens ID.");
            }

            // Henter respondenten og assessmentens slutdato.
            var respondent = await dbContext.Respondents
                .Include(r => r.Assessment)
                .FirstOrDefaultAsync(r => r.RespondentId == respondentId);

            if (respondent is null)
            {
                throw new InvalidOperationException(
                    "Respondenten blev ikke fundet.");
            }

            // Sessionen må ikke leve længere end assessmenten.
            var expires = GetExpiration(
                ticket,
                now,
                respondent.Assessment.EndDate);

            if (expires <= now)
            {
                throw new InvalidOperationException(
                    "Assessmenten er udløbet.");
            }

            ticket.Properties.ExpiresUtc = expires;

            // Opretter et tilfældigt session-ID.
            var id = new Guid(RandomNumberGenerator.GetBytes(16));

            // Serialiserer og beskytter authentication-ticketen.
            var serializedTicket = TicketSerializer.Default.Serialize(ticket);
            var protectedTicket = _protector.Protect(serializedTicket);

            var session = new RespondentSession
            {
                Id = id,
                RespondentId = respondentId,
                Ticket = protectedTicket,
                Created = now,
                LastActivity = now,
                Expires = expires
            };

            dbContext.RespondentSessions.Add(session);

            await dbContext.SaveChangesAsync();

            return id.ToString("N");
        }

        // Henter en eksisterende respondent-session.
        public async Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            if (!Guid.TryParseExact(key, "N", out var id))
            {
                return null;
            }

            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var session = await dbContext.RespondentSessions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session is null)
            {
                return null;
            }

            if (session.Revoked.HasValue ||
                session.Expires <= DateTime.UtcNow)
            {
                return null;
            }

            try
            {
                var serializedTicket = _protector.Unprotect(session.Ticket);

                var ticket = TicketSerializer.Default.Deserialize(serializedTicket);

                if (ticket is null)
                {
                    return null;
                }

                session.LastActivity = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();

                return ticket;
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        // Fornyer en eksisterende respondent-session.
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

            var session = await dbContext.RespondentSessions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session is null ||
                session.Revoked.HasValue ||
                session.Expires <= DateTime.UtcNow)
            {
                return;
            }

            var respondent = await dbContext.Respondents
                .Include(r => r.Assessment)
                .FirstOrDefaultAsync(r => r.RespondentId == session.RespondentId);

            if (respondent is null)
            {
                return;
            }

            var now = DateTime.UtcNow;

            var expires = GetExpiration(
                ticket,
                now,
                respondent.Assessment.EndDate);

            if (expires <= now)
            {
                return;
            }

            ticket.Properties.ExpiresUtc = expires;

            var serializedTicket = TicketSerializer.Default.Serialize(ticket);

            session.Ticket = _protector.Protect(serializedTicket);
            session.LastActivity = now;
            session.Expires = expires;

            await dbContext.SaveChangesAsync();
        }

        // Tilbagekalder en respondent-session.
        public async Task RemoveAsync(string key)
        {
            if (!Guid.TryParseExact(key, "N", out var id))
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var session = await dbContext.RespondentSessions
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session is null || session.Revoked.HasValue)
            {
                return;
            }

            session.Revoked = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
        }

        // Finder udløbstidspunktet, men aldrig senere end assessmentens slutdato.
        private static DateTime GetExpiration(
            AuthenticationTicket ticket,
            DateTime now,
            DateTime assessmentEndDate)
        {
            var ticketExpiration = ticket.Properties.ExpiresUtc?.UtcDateTime
                ?? now.AddHours(8);

            return ticketExpiration < assessmentEndDate
                ? ticketExpiration
                : assessmentEndDate;
        }
    }
}
