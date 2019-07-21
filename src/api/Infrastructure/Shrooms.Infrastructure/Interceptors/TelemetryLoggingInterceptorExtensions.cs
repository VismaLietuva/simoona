using Autofac.Builder;
using Autofac.Extras.DynamicProxy;

namespace Shrooms.Infrastructure.Interceptors
{
    public static class TelemetryLoggingInterceptorExtensions
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> EnableInterfaceTelemetryInterceptor<TLimit, TActivatorData, TSingleRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration)
        {
            return registration.EnableInterfaceInterceptors().InterceptedBy(typeof(TelemetryLoggingInterceptor));
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> EnableClassTelemetryInterceptor<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration)
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
        {
            return registration.EnableClassInterceptors().InterceptedBy(typeof(TelemetryLoggingInterceptor));
        }
    }
}