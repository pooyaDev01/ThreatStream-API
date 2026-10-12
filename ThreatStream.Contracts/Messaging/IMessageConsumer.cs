using System;
using System.Collections.Generic;
using System.Text;

namespace ThreatStream.Contracts.Messaging
{
    public interface IMessageConsumer
    {
    Task ConsumeAsync<T>(string topic,string consumerGroupId,Func<string?, T, Task> messageHandler,CancellationToken cancellationToken);
    }
}
