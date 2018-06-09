using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Lib
{
    public static class RabbitMQServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQ(
           this IServiceCollection services,
           Action<RabbitMQOptions> providerAction
            )        
        {
            services.AddOptions();
            services.Configure(providerAction);

            services.TryAddSingleton<IRabbitMQManager, RabbitMQManager>();            

            return services;
        }
    }
}
