using System;
using System.Collections.Generic;
using System.Text;

namespace Lib
{
    public interface IRabbitMQManager
    {
        void Publish<T>(T message, string queueName, string routeKey);

        void Consume<T>(string queueName, string routeKey, Action<T> action) where T : class;
    }
}
