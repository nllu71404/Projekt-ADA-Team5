using ADAProjectAPIVerticalSlice.Infrastructure.Authentication;
using ADAProjectAPIVerticalSlice.Infrastructure.Database;
using ADAProjectAPIVerticalSlice.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ADAProjectAPIVerticalSlice.Features.AuditHistory;

public static class GetAuditHistory
{
    public sealed record Response(
        long AuditId,
        DateTime ChangedAt,
        string AssessmentId,
        string Operation,
        string FieldName,
        string? OldValue,
        string? NewValue,
        string? ChangedBy,
        string? RequestId);

    public sealed record Query(Guid? AssessmentId)
        : IRequest<Result<List<Response>>>;

    public sealed class Handler(
        ApplicationDbContext db,
        ICurrentUserService currentUser)
        : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            var companyId = await db.Users
                .Where(user => user.Id == userId)
                .Select(user => (Guid?)user.CompanyId)
                .SingleOrDefaultAsync(cancellationToken);

            if (companyId is not Guid callerCompanyId)
            {
                return Result.Failure<List<Response>>(new Error(
                    "GetAuditHistory.Unauthorized",
                    "Sign in to view audit history."));
            }

            // Firmaet bestemmes af den loggede bruger på serveren.
            var changes = db.AssessmentAuditChanges
                .AsNoTracking()
                .Where(change => change.CompanyId == callerCompanyId);

            if (request.AssessmentId is Guid assessmentId)
            {
                var assessmentKey = assessmentId.ToString("D");
                changes = changes.Where(change =>
                    change.AssessmentId == assessmentKey);
            }

            var rows = await changes
                .OrderByDescending(change => change.AuditId)
                .ThenBy(change => change.FieldName)
                .Take(200)
                .Select(change => new Response(
                    change.AuditId,
                    change.ChangedAt,
                    change.AssessmentId,
                    change.Operation,
                    change.FieldName,
                    change.OldValue,
                    change.NewValue,
                    db.Users
                        .Where(actor => actor.Id == change.ActorId &&
                            actor.CompanyId == callerCompanyId)
                        .Select(actor => actor.UserName)
                        .FirstOrDefault(),
                    change.RequestId))
                .ToListAsync(cancellationToken);

            return Result.Success(rows);
        }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/api/audit-history",
                async (
                    Guid? assessmentId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new Query(assessmentId), cancellationToken);

                    if (result.IsFailure)
                    {
                        return Results.Unauthorized();
                    }

                    return Results.Ok(result.Value);
                })
                .RequireAuthorization("AdminOnly");
        }
    }
}