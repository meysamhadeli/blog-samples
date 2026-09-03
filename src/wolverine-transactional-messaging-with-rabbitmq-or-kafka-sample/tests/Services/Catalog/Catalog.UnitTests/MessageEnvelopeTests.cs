using Tests.Shared;

namespace Catalog.UnitTests;

public class MessageEnvelopeTests
{
    [Fact]
    public void Create_ShouldPopulate_Metadata_AndPayload()
    {
        var correlationId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var messageId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var envelope = SampleData.ProductCreatedEnvelope(correlationId, messageId);

        Assert.Equal(messageId, envelope.MessageId);
        Assert.Equal(correlationId, envelope.CorrelationId);
        Assert.Equal(SampleData.CreatedAtUtc, envelope.OccurredAtUtc);
        Assert.Equal(SampleData.ProductId, envelope.Message.Id);
        Assert.Equal(SampleData.ProductCreatedEnvelope().Message.Name, envelope.Message.Name);
        Assert.Equal(SampleData.ProductCreatedEnvelope().Message.Price, envelope.Message.Price);
    }
}
