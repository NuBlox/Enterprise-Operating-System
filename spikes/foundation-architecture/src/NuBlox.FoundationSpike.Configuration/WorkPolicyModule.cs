using System.Data.Common;
using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Configuration;

public sealed record WorkPolicyVersion(
    Guid VersionId,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    bool ApprovalRequired,
    int MaxOpenWork,
    bool ExternalCollaborationEnabled,
    bool AuditRequired,
    string ChangeReason,
    DateTimeOffset RecordedAt);

public sealed class WorkPolicyModule
{
    public async Task<Guid> ActivateAsync(
        ITransactionalSession session,
        CustomerId customerId,
        bool approvalRequired,
        int maxOpenWork,
        bool externalCollaborationEnabled,
        DateTimeOffset effectiveFrom,
        string changeReason,
        CancellationToken cancellationToken = default)
    {
        if (maxOpenWork is < 1 or > 10000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxOpenWork),
                maxOpenWork,
                "maxOpenWork must be between 1 and 10000 for this spike.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(changeReason);

        await using (var closeCommand = session.Connection.CreateCommand())
        {
            closeCommand.Transaction = session.Transaction;
            closeCommand.CommandText = """
                UPDATE configuration.work_policy_versions
                SET effective_to = @effective_from
                WHERE customer_id = @customer_id
                  AND effective_to IS NULL
                  AND effective_from < @effective_from;
                """;
            closeCommand.AddParameter("customer_id", customerId.Value);
            closeCommand.AddParameter("effective_from", effectiveFrom);
            await closeCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        var versionId = Guid.NewGuid();

        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            INSERT INTO configuration.work_policy_versions
                (
                    customer_id,
                    version_id,
                    effective_from,
                    effective_to,
                    approval_required,
                    max_open_work,
                    external_collaboration_enabled,
                    audit_required,
                    change_reason
                )
            VALUES
                (
                    @customer_id,
                    @version_id,
                    @effective_from,
                    NULL,
                    @approval_required,
                    @max_open_work,
                    @external_collaboration_enabled,
                    true,
                    @change_reason
                );
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("version_id", versionId);
        command.AddParameter("effective_from", effectiveFrom);
        command.AddParameter("approval_required", approvalRequired);
        command.AddParameter("max_open_work", maxOpenWork);
        command.AddParameter("external_collaboration_enabled", externalCollaborationEnabled);
        command.AddParameter("change_reason", changeReason.Trim());
        await command.ExecuteNonQueryAsync(cancellationToken);

        return versionId;
    }

    public async Task<WorkPolicyVersion?> GetAsOfAsync(
        ITransactionalSession session,
        CustomerId customerId,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken = default)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT
                version_id,
                effective_from,
                effective_to,
                approval_required,
                max_open_work,
                external_collaboration_enabled,
                audit_required,
                change_reason,
                recorded_at
            FROM configuration.work_policy_versions
            WHERE customer_id = @customer_id
              AND effective_from <= @effective_at
              AND (effective_to IS NULL OR @effective_at < effective_to)
            ORDER BY effective_from DESC
            LIMIT 1;
            """;
        command.AddParameter("customer_id", customerId.Value);
        command.AddParameter("effective_at", effectiveAt);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? Read(reader)
            : null;
    }

    public async Task<IReadOnlyList<WorkPolicyVersion>> GetAllVersionsAsync(
        ITransactionalSession session,
        CustomerId customerId,
        CancellationToken cancellationToken = default)
    {
        await using var command = session.Connection.CreateCommand();
        command.Transaction = session.Transaction;
        command.CommandText = """
            SELECT
                version_id,
                effective_from,
                effective_to,
                approval_required,
                max_open_work,
                external_collaboration_enabled,
                audit_required,
                change_reason,
                recorded_at
            FROM configuration.work_policy_versions
            WHERE customer_id = @customer_id
            ORDER BY effective_from;
            """;
        command.AddParameter("customer_id", customerId.Value);

        var versions = new List<WorkPolicyVersion>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            versions.Add(Read(reader));
        }

        return versions;
    }

    private static WorkPolicyVersion Read(DbDataReader reader)
        => new(
            reader.GetGuid(0),
            new DateTimeOffset(reader.GetFieldValue<DateTime>(1)),
            reader.IsDBNull(2) ? null : new DateTimeOffset(reader.GetFieldValue<DateTime>(2)),
            reader.GetBoolean(3),
            reader.GetInt32(4),
            reader.GetBoolean(5),
            reader.GetBoolean(6),
            reader.GetString(7),
            new DateTimeOffset(reader.GetFieldValue<DateTime>(8)));
}
