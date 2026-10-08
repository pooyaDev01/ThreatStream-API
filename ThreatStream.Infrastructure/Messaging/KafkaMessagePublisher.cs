using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Infrastructure.Configuration;

namespace ThreatStream.Infrastructure.Messaging
{
    public class KafkaMessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaMessagePublisher(IOptions<KafkaOptions> options)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task PublishAsync<T>(T message, string topic)
        {
            var json = JsonSerializer.Serialize(message);

            await _producer.ProduceAsync(
                topic,
                new Message<Null, string>
                {
                    Value = json
                });
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
