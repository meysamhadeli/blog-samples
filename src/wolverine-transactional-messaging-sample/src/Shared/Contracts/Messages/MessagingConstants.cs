namespace Contracts.Messages.Constants;

public static class MessagingConstants
{
    public const string RabbitMqTransport = "rabbitmq";
    public const string KafkaTransport = "kafka";

    public static class Exchanges
    {
        public const string CatalogExchange = "catalog-exchange";
    }

    public static class Queues
    {
        public const string CatalogQueue = "catalog-queue";
        public const string OrderQueue = "order-queue";
        public const string CatalogReadModelQueue = "catalog-read-model-queue";
    }

    public static class Topics
    {
        public const string ProductCreatedTopic = "product-created-topic";
    }

    public static class Groups
    {
        public const string OrderGroup = "order-group";
        public const string CatalogGroup = "catalog-group";
    }

    /// <summary>
    /// Module name prefixes used for Wolverine exchange/queue auto-naming.
    /// Matches the auto-derived prefix from assembly name (last segment, lowercased).
    /// Shared so consumers can reference publisher's prefix without hardcoding.
    /// </summary>
    public static class ModulePrefixes
    {
        public const string Catalog = "catalog";
        public const string Order = "order";
    }
}
