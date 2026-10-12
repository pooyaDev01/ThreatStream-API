using System;
using System.Collections.Generic;
using System.Text;

namespace ThreatStream.Contracts.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string topic, string? key = null);
    }
}
