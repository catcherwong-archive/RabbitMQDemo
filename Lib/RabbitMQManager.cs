using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Lib
{
    public class RabbitMQManager : IRabbitMQManager
    {
        private readonly RabbitMQOptions _options;

        private readonly IConnection _connection;

        public RabbitMQManager(IOptionsMonitor<RabbitMQOptions> options)
        {
            _options = options.CurrentValue;

            var factory = new ConnectionFactory()
            {
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password
            };

            //创建连接
            _connection = factory.CreateConnection();           
        }

        public void Consume<T>(string queueName, string routeKey, Action<T> action) where T : class
        {
            //创建通道
            var channel = _connection.CreateModel();

            //声明一个队列 (durable=true 持久化消息）
            channel.QueueDeclare(queueName, true, false, false, null);

            if (!string.IsNullOrEmpty(_options.ExchangeName))
            {
                channel.ExchangeDeclare(_options.ExchangeName, _options.ExchangeType, false, false, null);
                //将队列绑定到交换机
                channel.QueueBind(queueName, _options.ExchangeName, routeKey, null);
            }

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);            

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var obj = JsonConvert.DeserializeObject<T>(message);

                try
                {                    
                    action?.Invoke(obj);                    
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);                                   
            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);            
        }

        public void Publish<T>(T message, string queueName, string routeKey)
        {
            if (message == null || string.IsNullOrWhiteSpace(queueName))
                return;            

            //创建通道
            var channel = _connection.CreateModel();

            //声明一个队列 (durable=true 持久化消息）
            channel.QueueDeclare(queueName, true, false, false, null);

            if (!string.IsNullOrEmpty(_options.ExchangeName))
            {
                channel.ExchangeDeclare(_options.ExchangeName, _options.ExchangeType, false, false, null);
                //将队列绑定到交换机
                channel.QueueBind(queueName, _options.ExchangeName, routeKey, null);
            }

            var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(_options.ExchangeName, routeKey, properties, sendBytes);
        }
    }
}
