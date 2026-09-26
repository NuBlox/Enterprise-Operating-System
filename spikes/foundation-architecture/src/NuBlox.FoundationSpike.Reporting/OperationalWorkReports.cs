using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Reporting;

public sealed record WorkReportDefinition(
    string Code,
    string Version,
    string Owner,
    string Source,
    string Calculation,
    string TimeBasis);

public sealed record WorkReportRow(
    WorkId WorkId,
    SubjectId SubjectId,
    string Summary,
    DateTimeOffset CreatedAt);

/// <summary>
/// Read-only, customer-scoped operational queries over the Work module's
/// authoritative table. The caller must supply a restricted customer session.
/// </summary>
public sealed class OperationalWorkReports
{
    public static readonly WorkReportDefinition CreatedThroughDefinition = new(
        "WORK_CREATED_THROUGH", "spike-0.1", "NuBlox Architecture / Engineering",
        "work.work_requests", "COUNT(work requests with created_at < exclusive UTC cutoff)",
        "Technical creation time; cumulative through the exclusive cutoff. State is not reconstructed.");

    public static readonly WorkReportDefinition CurrentOpenDefinition = new(
        "WORK_CURRENT_OPEN", "spike-0.1", "NuBlox Architecture / Engineering",
        "work.work_requests", "COUNT(work requests with state = OPEN)",
        "Current committed state at query time; no historical-state claim.");

    private const string CreatedCountSql = """
        SELECT count(*)
        FROM work.work_requests
        WHERE customer_id = @customer_id
          AND created_at < @before_exclusive;
        """;

    private const string CreatedDetailsSql = """
        SELECT id, subject_id, summary, created_at
        FROM work.work_requests
        WHERE customer_id = @customer_id
          AND created_at < @before_exclusive
        ORDER BY created_at, id
        LIMIT @page_size OFFSET @offset;
        """;

    private const string OpenCountSql = """
        SELECT count(*)
        FROM work.work_requests
        WHERE customer_id = @customer_id
          AND state = 'OPEN';
        """;

    private const string OpenDetailsSql = """
        SELECT id, subject_id, summary, created_at
        FROM work.work_requests
        WHERE customer_id = @customer_id
          AND state = 'OPEN'
        ORDER BY id
        LIMIT @page_size OFFSET @offset;
        """;

    public Task<long> CountCreatedThroughAsync(
        ITransactionalSession session,
        CustomerId customerId,
        DateTimeOffset beforeExclusive,
        CancellationToken cancellationToken = default) =>
        CountAsync(session, customerId, CreatedCountSql, beforeExclusive, cancellationToken);

    public Task<long> CountCurrentOpenAsync(
        ITransactionalSession session,
        CustomerId customerId,
        CancellationToken cancellationToken = default) =>
        CountAsync(session, customerId, OpenCountSql, null, cancellationToken);

    public Task<IReadOnlyList<WorkReportRow>> ListCreatedThroughAsync(
        ITransactionalSession session,
        CustomerId customerId,
        DateTimeOffset beforeExclusive,
        int offset,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        ListAsync(session, customerId, CreatedDetailsSql, beforeExclusive, offset, pageSize, cancellationToken);

    public Task<IReadOnlyList<WorkReportRow>> ListCurrentOpenAsync(
        ITransactionalSession session,
        CustomerId customerId,
        int offset,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        ListAsync(session, customerId, OpenDetailsSql, null, offset, pageSize, cancellationToken);

    // The same parameterised queries are explained at synthetic volume by the
    // verifier; plans and timings are printed as evidence for manual review.
    public async Task<string> ExplainCreatedThroughAsync(
        ITransactionalSession session,
        CustomerId customerId,
        DateTimeOffset beforeExclusive,
        bool details,
        CancellationToken cancellationToken = default)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = "EXPLAIN (ANALYZE, BUFFERS) " + (details ? CreatedDetailsSql : CreatedCountSql);
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("before_exclusive", beforeExclusive.UtcDateTime);
        if (details)
        {
            command.AddParameter("page_size", 50);
            command.AddParameter("offset", 0);
        }

        var lines = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            lines.Add(reader.GetString(0));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static async Task<long> CountAsync(
        ITransactionalSession session,
        CustomerId customerId,
        string sql,
        DateTimeOffset? beforeExclusive,
        CancellationToken cancellationToken)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = sql;
        command.AddParameter("customer_id", customerId.Value);
        if (beforeExclusive is { } cutoff)
        {
            command.AddParameter("before_exclusive", cutoff.UtcDateTime);
        }

        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task<IReadOnlyList<WorkReportRow>> ListAsync(
        ITransactionalSession session,
        CustomerId customerId,
        string sql,
        DateTimeOffset? beforeExclusive,
        int offset,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (offset < 0 || pageSize is < 1 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Offset must be non-negative and page size must be between 1 and 200.");
        }

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = sql;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("offset", offset);
        command.AddParameter("page_size", pageSize);
        if (beforeExclusive is { } cutoff)
        {
            command.AddParameter("before_exclusive", cutoff.UtcDateTime);
        }

        var rows = new List<WorkReportRow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new WorkReportRow(
                new WorkId(reader.GetGuid(0)),
                new SubjectId(reader.GetGuid(1)),
                reader.GetString(2),
                new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc))));
        }

        return rows;
    }
}
