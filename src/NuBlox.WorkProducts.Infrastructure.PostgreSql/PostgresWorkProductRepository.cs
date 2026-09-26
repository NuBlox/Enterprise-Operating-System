using Npgsql;
using NuBlox.Kernel;
using NuBlox.Persistence.PostgreSql;
using NuBlox.WorkProducts.Application;

namespace NuBlox.WorkProducts.Infrastructure.PostgreSql;

public sealed class PostgresWorkProductRepository : IWorkProductRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresWorkProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task AddAsync(WorkProductRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (record.WorkProduct.TenantId != record.CurrentRevision.TenantId
            || record.WorkProduct.Id != record.CurrentRevision.WorkProductId)
        {
            throw new InvalidOperationException("Work Product and revision identities must match.");
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, record.WorkProduct.TenantId.Value, cancellationToken).ConfigureAwait(false);

        await using (var command = new NpgsqlCommand(
            """
            INSERT INTO work_products.work_products (
                tenant_id, work_product_id, title, product_type,
                owner_principal_id, created_by_principal_id, created_at,
                lifecycle, current_revision_number)
            VALUES (
                @tenant_id, @work_product_id, @title, @product_type,
                @owner_principal_id, @created_by_principal_id, @created_at,
                @lifecycle, @current_revision_number);
            """,
            connection,
            transaction))
        {
            AddWorkProductParameters(command, record.WorkProduct);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using (var command = new NpgsqlCommand(
            """
            INSERT INTO work_products.revisions (
                tenant_id, work_product_revision_id, work_product_id, revision_number,
                title_snapshot, state, created_by_principal_id, created_at,
                submitted_by_principal_id, submitted_at,
                issued_by_principal_id, issued_at)
            VALUES (
                @tenant_id, @revision_id, @work_product_id, @revision_number,
                @title_snapshot, @state, @created_by_principal_id, @created_at,
                @submitted_by_principal_id, @submitted_at,
                @issued_by_principal_id, @issued_at);
            """,
            connection,
            transaction))
        {
            AddRevisionParameters(command, record.CurrentRevision);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<WorkProductRecord?> FindAsync(
        TenantId tenantId,
        WorkProductId workProductId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, tenantId.Value, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(
            """
            SELECT
                p.work_product_id, p.title, p.product_type, p.owner_principal_id,
                p.created_by_principal_id, p.created_at, p.lifecycle, p.current_revision_number,
                r.work_product_revision_id, r.revision_number, r.title_snapshot,
                r.state, r.created_by_principal_id, r.created_at,
                r.submitted_by_principal_id, r.submitted_at,
                r.issued_by_principal_id, r.issued_at
            FROM work_products.work_products p
            JOIN work_products.revisions r
              ON r.tenant_id = p.tenant_id
             AND r.work_product_id = p.work_product_id
             AND r.revision_number = p.current_revision_number
            WHERE p.tenant_id = @tenant_id
              AND p.work_product_id = @work_product_id;
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("tenant_id", tenantId.Value);
        command.Parameters.AddWithValue("work_product_id", workProductId.Value);

        WorkProductRecord? result = null;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var workProduct = WorkProduct.Restore(
                    new WorkProductId(reader.GetGuid(0)), tenantId, reader.GetString(1), reader.GetString(2),
                    new PrincipalId(reader.GetGuid(3)), new PrincipalId(reader.GetGuid(4)),
                    reader.GetFieldValue<DateTimeOffset>(5), ParseLifecycle(reader.GetString(6)), reader.GetInt32(7));

                var submittedBy = reader.IsDBNull(14) ? (PrincipalId?)null : new PrincipalId(reader.GetGuid(14));
                var submittedAt = reader.IsDBNull(15) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(15);
                var issuedBy = reader.IsDBNull(16) ? (PrincipalId?)null : new PrincipalId(reader.GetGuid(16));
                var issuedAt = reader.IsDBNull(17) ? (DateTimeOffset?)null : reader.GetFieldValue<DateTimeOffset>(17);
                var revision = WorkProductRevision.Restore(
                    new WorkProductRevisionId(reader.GetGuid(8)), tenantId, workProduct.Id,
                    reader.GetInt32(9), reader.GetString(10), ParseRevisionState(reader.GetString(11)),
                    new PrincipalId(reader.GetGuid(12)), reader.GetFieldValue<DateTimeOffset>(13),
                    submittedBy, submittedAt, issuedBy, issuedAt);

                result = new WorkProductRecord(workProduct, revision);
            }
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task SubmitForReviewAsync(
        ReviewSubmission submission,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(submission);
        if (submission.Revision.TenantId != submission.ReviewRequest.TenantId
            || submission.Revision.Id != submission.ReviewRequest.WorkProductRevisionId
            || submission.Revision.State != WorkProductRevisionState.InReview)
        {
            throw new InvalidOperationException("Review submission identities or state are inconsistent.");
        }

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await PostgresTenantSession.SetTenantAsync(connection, transaction, submission.Revision.TenantId.Value, cancellationToken).ConfigureAwait(false);

        await using (var update = new NpgsqlCommand(
            """
            UPDATE work_products.revisions
               SET state = 'IN_REVIEW',
                   submitted_by_principal_id = @submitted_by,
                   submitted_at = @submitted_at
             WHERE tenant_id = @tenant_id
               AND work_product_revision_id = @revision_id
               AND state = 'DRAFT';
            """,
            connection,
            transaction))
        {
            update.Parameters.AddWithValue("submitted_by", submission.Revision.SubmittedByPrincipalId!.Value.Value);
            update.Parameters.AddWithValue("submitted_at", submission.Revision.SubmittedAtUtc!.Value);
            update.Parameters.AddWithValue("tenant_id", submission.Revision.TenantId.Value);
            update.Parameters.AddWithValue("revision_id", submission.Revision.Id.Value);
            if (await update.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
            {
                throw new WorkProductStateConflictException("The revision is no longer Draft and cannot be submitted for review.");
            }
        }

        await using (var insert = new NpgsqlCommand(
            """
            INSERT INTO work_products.review_requests (
                tenant_id, review_request_id, work_product_revision_id, kind,
                requested_principal_id, requested_by_principal_id, requested_at, state)
            VALUES (
                @tenant_id, @request_id, @revision_id, 'REVIEW',
                @requested_principal, @requested_by, @requested_at, 'OPEN');
            """,
            connection,
            transaction))
        {
            insert.Parameters.AddWithValue("tenant_id", submission.ReviewRequest.TenantId.Value);
            insert.Parameters.AddWithValue("request_id", submission.ReviewRequest.Id.Value);
            insert.Parameters.AddWithValue("revision_id", submission.ReviewRequest.WorkProductRevisionId.Value);
            insert.Parameters.AddWithValue("requested_principal", submission.ReviewRequest.RequestedPrincipalId.Value);
            insert.Parameters.AddWithValue("requested_by", submission.ReviewRequest.RequestedByPrincipalId.Value);
            insert.Parameters.AddWithValue("requested_at", submission.ReviewRequest.RequestedAtUtc);
            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void AddWorkProductParameters(NpgsqlCommand command, WorkProduct workProduct)
    {
        command.Parameters.AddWithValue("tenant_id", workProduct.TenantId.Value);
        command.Parameters.AddWithValue("work_product_id", workProduct.Id.Value);
        command.Parameters.AddWithValue("title", workProduct.Title);
        command.Parameters.AddWithValue("product_type", workProduct.ProductType);
        command.Parameters.AddWithValue("owner_principal_id", workProduct.OwnerPrincipalId.Value);
        command.Parameters.AddWithValue("created_by_principal_id", workProduct.CreatedByPrincipalId.Value);
        command.Parameters.AddWithValue("created_at", workProduct.CreatedAtUtc);
        command.Parameters.AddWithValue("lifecycle", FormatLifecycle(workProduct.Lifecycle));
        command.Parameters.AddWithValue("current_revision_number", workProduct.CurrentRevisionNumber);
    }

    private static void AddRevisionParameters(NpgsqlCommand command, WorkProductRevision revision)
    {
        command.Parameters.AddWithValue("tenant_id", revision.TenantId.Value);
        command.Parameters.AddWithValue("revision_id", revision.Id.Value);
        command.Parameters.AddWithValue("work_product_id", revision.WorkProductId.Value);
        command.Parameters.AddWithValue("revision_number", revision.RevisionNumber);
        command.Parameters.AddWithValue("title_snapshot", revision.TitleSnapshot);
        command.Parameters.AddWithValue("state", FormatRevisionState(revision.State));
        command.Parameters.AddWithValue("created_by_principal_id", revision.CreatedByPrincipalId.Value);
        command.Parameters.AddWithValue("created_at", revision.CreatedAtUtc);
        command.Parameters.AddWithValue("submitted_by_principal_id", revision.SubmittedByPrincipalId is { } submittedBy ? submittedBy.Value : DBNull.Value);
        command.Parameters.AddWithValue("submitted_at", revision.SubmittedAtUtc is { } submittedAt ? submittedAt : DBNull.Value);
        command.Parameters.AddWithValue("issued_by_principal_id", revision.IssuedByPrincipalId is { } issuedBy ? issuedBy.Value : DBNull.Value);
        command.Parameters.AddWithValue("issued_at", revision.IssuedAtUtc is { } issuedAt ? issuedAt : DBNull.Value);
    }

    private static string FormatLifecycle(WorkProductLifecycle lifecycle) => lifecycle switch
    {
        WorkProductLifecycle.Active => "ACTIVE",
        WorkProductLifecycle.Superseded => "SUPERSEDED",
        WorkProductLifecycle.Withdrawn => "WITHDRAWN",
        _ => throw new ArgumentOutOfRangeException(nameof(lifecycle))
    };

    private static WorkProductLifecycle ParseLifecycle(string lifecycle) => lifecycle switch
    {
        "ACTIVE" => WorkProductLifecycle.Active,
        "SUPERSEDED" => WorkProductLifecycle.Superseded,
        "WITHDRAWN" => WorkProductLifecycle.Withdrawn,
        _ => throw new InvalidOperationException($"Unknown Work Product lifecycle '{lifecycle}'.")
    };

    private static string FormatRevisionState(WorkProductRevisionState state) => state switch
    {
        WorkProductRevisionState.Draft => "DRAFT",
        WorkProductRevisionState.InReview => "IN_REVIEW",
        WorkProductRevisionState.ChangesRequired => "CHANGES_REQUIRED",
        WorkProductRevisionState.Rejected => "REJECTED",
        WorkProductRevisionState.Approved => "APPROVED",
        WorkProductRevisionState.Issued => "ISSUED",
        WorkProductRevisionState.Superseded => "SUPERSEDED",
        _ => throw new ArgumentOutOfRangeException(nameof(state))
    };

    private static WorkProductRevisionState ParseRevisionState(string state) => state switch
    {
        "DRAFT" => WorkProductRevisionState.Draft,
        "IN_REVIEW" => WorkProductRevisionState.InReview,
        "CHANGES_REQUIRED" => WorkProductRevisionState.ChangesRequired,
        "REJECTED" => WorkProductRevisionState.Rejected,
        "APPROVED" => WorkProductRevisionState.Approved,
        "ISSUED" => WorkProductRevisionState.Issued,
        "SUPERSEDED" => WorkProductRevisionState.Superseded,
        _ => throw new InvalidOperationException($"Unknown Work Product revision state '{state}'.")
    };
}
