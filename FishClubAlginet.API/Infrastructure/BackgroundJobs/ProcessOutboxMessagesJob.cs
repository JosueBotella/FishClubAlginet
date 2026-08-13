namespace FishClubAlginet.API.Infrastructure.BackgroundJobs;

public class ProcessOutboxMessagesJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDomainEventTypeResolver _typeResolver;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(
        IServiceProvider serviceProvider,
        IDomainEventTypeResolver typeResolver,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _serviceProvider = serviceProvider;
        _typeResolver = typeResolver;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Runs every 10 seconds
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error while processing the Outbox.");
            }
        }
    }

    private async Task ProcessMessages(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        // Get unprocessed messages in FIFO order (First In, First Out)
        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc) // FIFO: oldest first
            .Take(20)
            .ToListAsync(stoppingToken);

        if (!messages.Any())
            return;

        foreach (var outboxMessage in messages)
        {
            try
            {
                // Resuelve dinámicamente el tipo a través del resolver optimizado con caché (O(1))
                // Funciona con eventos en cualquier namespace / bounded context y admite tanto
                // nombre simple como nombre completo/AssemblyQualifiedName.
                var type = _typeResolver.Resolve(outboxMessage.Type);

                if (type is null)
                {
                    _logger.LogWarning("IDomainEvent type not found for name: {TypeName}", outboxMessage.Type);
                    outboxMessage.Error = $"Type not found: {outboxMessage.Type}";
                    continue;
                }

                // Deserialize the JSON back to the original object
                var deserialized = JsonSerializer.Deserialize(outboxMessage.Content, type);

                if (deserialized is not IDomainEvent domainEvent)
                {
                    _logger.LogWarning("Deserialized object is not an IDomainEvent: {Type}", type.Name);
                    outboxMessage.Error = "Not an IDomainEvent";
                    continue;
                }

                // Publish the domain event (handlers will process it)
                await publisher.Publish(domainEvent, stoppingToken);

                // Mark as successfully processed
                outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
                _logger.LogInformation("Outbox message processed successfully: {MessageId} ({EventType})", 
                    outboxMessage.Id, outboxMessage.Type);
            }
            catch (Exception ex)
            {
                // If processing fails, save the error and retry on next execution
                outboxMessage.Error = ex.Message;
                _logger.LogError(ex, "Error processing OutboxMessage {MessageId}: {ErrorMessage}", 
                    outboxMessage.Id, ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
