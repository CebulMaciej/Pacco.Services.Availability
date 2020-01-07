using Convey;
using Convey.Logging.CQRS;
using Microsoft.Extensions.DependencyInjection;
using Pacco.Services.Availability.Application;

namespace Pacco.Services.Availability.Infrastructure.Logging
{
    internal static class Extensions
    {
        public static IConveyBuilder AddHandlersLogging(this IConveyBuilder builder)
        {
            var assembly = typeof(IAppContext).Assembly;
            
            builder.Services.AddSingleton<IMessageToLogTemplateMapper>(new MessageToLogTemplateMapper());
            
            return builder
                .AddCommandHandlersLogging(assembly)
                .AddEventHandlersLogging(assembly);
        }
    }
}