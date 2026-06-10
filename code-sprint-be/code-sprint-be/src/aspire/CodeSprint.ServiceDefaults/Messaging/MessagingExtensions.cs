using CodeSprint.ServiceDefaults.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Wires up the CodeSprint messaging transport for a service. Call after
/// <c>AddServiceDefaults()</c> in a service's <c>Program.cs</c>.
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    /// The Aspire connection name for the RabbitMQ resource. MUST match the resource
    /// name passed to <c>builder.AddRabbitMQ("messaging")</c> in the AppHost.
    /// </summary>
    private const string MessagingConnectionName = "messaging";

    /// <summary>
    /// Registers the RabbitMQ client (shared <c>IConnection</c>) via the Aspire
    /// integration and the <see cref="IIntegrationEventPublisher"/> transport.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The same builder for chaining.</returns>
    public static TBuilder AddCodeSprintMessaging<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        // Aspire reads the "messaging" connection string injected by the AppHost
        // reference and registers a singleton IConnection in DI.
        builder.AddRabbitMQClient(MessagingConnectionName);

        builder.Services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        return builder;
    }
}
