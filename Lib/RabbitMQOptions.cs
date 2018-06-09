using _ExchangeType = RabbitMQ.Client.ExchangeType;

namespace Lib
{
    public class RabbitMQOptions
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string HostName { get; set; }

        public string ExchangeType { get; set; } = _ExchangeType.Topic;

        public string ExchangeName { get; set; }
    }
}
