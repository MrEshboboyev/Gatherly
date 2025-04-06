using Gatherly.Domain.Primitives;
using Gatherly.Persistence;
using Gatherly.Persistence.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using Quartz;

namespace Gatherly.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob(
    ApplicationDbContext dbContext,
    IPublisher publisher) : IJob
{
    /// <summary> 
    /// Executes the job to process outbox messages. 
    /// </summary> 
    /// <param name="context">The job execution context.</param>
    public async Task Execute(IJobExecutionContext context)
    {
        #region Get unprocessed messages

        // Retrieve unprocessed outbox messages
        var messages = await dbContext
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc) // Ensure consistent ordering
            .Take(20)
            .ToListAsync(context.CancellationToken);

        #endregion

        #region Process outbox messages

        // Process each outbox message
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3,
                attempt => TimeSpan.FromMilliseconds(50 * attempt));


        foreach (var outboxMessage in messages)
        {
            // Deserialize the domain event from the message content
            var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                outboxMessage.Content,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });

            if (domainEvent is null)
            {
                continue;
            }

            // Execute the publish operation with retry policy
            var result = await policy.ExecuteAndCaptureAsync(() =>
                publisher.Publish(domainEvent, context.CancellationToken));

            // Record any errors that occurred during publishing
            outboxMessage.Error = result.FinalException?.ToString();

            // Mark the outbox message as processed
            outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
        }

        #endregion

        // Save changes to the database
        await dbContext.SaveChangesAsync();
    }
}
