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
    public class KafkaMessageConsumer : IMessageConsumer
    {
        private readonly KafkaOptions _options;

        public KafkaMessageConsumer(IOptions<KafkaOptions> options)
        {
            _options = options.Value;
        }

        public async Task ConsumeAsync<T>(string topic, string consumerGroupId, Func<T, Task> messageHandler, CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = consumerGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

            consumer.Subscribe(topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(cancellationToken);

                        var message = JsonSerializer.Deserialize<T>(result.Message.Value);

                        if (message is null)
                        {
                            throw new JsonException($"Kafka message from topic '{topic}' deserialized to null.");
                        }

                        await messageHandler(message);

                        consumer.Commit(result);
                    }

                    catch (ConsumeException ex)
                    {
                        Console.Error.WriteLine($"Kafka consume error on topic '{topic}': {ex.Error.Reason}");
                    }
                }
            }

            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Expected during graceful shutdown.
            }

            finally
            {
                consumer.Close();
            }
        }
    }
}
