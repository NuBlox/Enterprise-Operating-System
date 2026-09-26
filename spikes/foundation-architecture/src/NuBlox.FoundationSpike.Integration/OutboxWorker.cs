using NuBlox.FoundationSpike.Shared;

namespace NuBlox.FoundationSpike.Integration;

public enum DeliveryOutcome
{
    Success,
    TransientFailure,
    PermanentFailure
}

public sealed record DeliveryResult(
    DeliveryOutcome Outcome,
    string? ProviderRequestId = null,
    string? Error = null);

public interface IExternalDeliveryClient
{
    Task<DeliveryResult> DeliverAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default);
}

public sealed class OutboxWorker(
    ICustomerScopedTransactionalSessionFactory sessionFactory,
    OutboxModule outbox,
    IExternalDeliveryClient deliveryClient)
{
    public async Task<OutboxMessageId?> ProcessOneAsync(
        CustomerId customerId,
        string workerId,
        TimeSpan retryDelay,
        CancellationToken cancellationToken = default)
    {
        OutboxMessage? message;

        await using (var claimSession = await sessionFactory.OpenAsync(customerId, cancellationToken))
        {
            message = await outbox.ClaimNextAsync(
                claimSession,
                customerId,
                workerId,
                cancellationToken);

            await claimSession.CommitAsync(cancellationToken);
        }

        if (message is null)
        {
            return null;
        }

        DeliveryResult result;
        try
        {
            result = await deliveryClient.DeliverAsync(message, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            result = new DeliveryResult(
                DeliveryOutcome.TransientFailure,
                Error: $"{exception.GetType().Name}: {exception.Message}");
        }

        await using var resultSession = await sessionFactory.OpenAsync(customerId, cancellationToken);

        switch (result.Outcome)
        {
            case DeliveryOutcome.Success:
                if (string.IsNullOrWhiteSpace(result.ProviderRequestId))
                {
                    throw new InvalidOperationException(
                        "Successful external delivery must provide a provider request identifier.");
                }

                await outbox.MarkSucceededAsync(
                    resultSession,
                    customerId,
                    message.Id,
                    result.ProviderRequestId,
                    cancellationToken);
                break;

            case DeliveryOutcome.TransientFailure:
                await outbox.MarkRetryAsync(
                    resultSession,
                    customerId,
                    message.Id,
                    result.Error ?? "Transient provider failure",
                    DateTimeOffset.UtcNow.Add(retryDelay),
                    cancellationToken);
                break;

            case DeliveryOutcome.PermanentFailure:
                await outbox.MarkFailedActionRequiredAsync(
                    resultSession,
                    customerId,
                    message.Id,
                    result.Error ?? "Permanent provider rejection",
                    result.ProviderRequestId,
                    cancellationToken);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(result.Outcome), result.Outcome, null);
        }

        await resultSession.CommitAsync(cancellationToken);
        return message.Id;
    }
}
