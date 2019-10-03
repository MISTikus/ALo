using ALo.Infrastructure.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Xunit;

namespace ALo.Infrastructure.Tests.Pipelines
{
    public class PipelineExamples
    {
        [Fact]
        public void Pipeline_With_ServiceCollection()
        {
            var services = new Mock<IServiceCollection>();
            services.Setup(x => x.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(JsonContext)))).Callback(() => Impossible());
            object localVariable = null;
            var databaseOptions = new { Type = "Sql" }; // Some kind of enumeration, enums are better, but everything fits

            services.Object
                .Do(s => localVariable = new object()) // Initialize local variable to use in next steps
                .If(localVariable is null, s => Impossible()) // Impossible because we already assigned this variable
                .Do(s => s.Switch(databaseOptions.Type, // Chose database connection by provider type
                    ("Sql", s => s.AddTransient<SqlContext>()),
                    ("Json", s => s.AddTransient<JsonContext>())
                ))
                .Extract(s => BuildProvider(s).GetService<IAppRunner>()) // Building provider
                .Run(); // Running the app
            ;
        }

        private IServiceProvider BuildProvider(IServiceCollection services) => new Mock<IServiceProvider>()
            .Do(m => m
                .Setup(x => x.GetService(It.Is<Type>(t => t == typeof(IAppRunner))))
                .Returns(new Mock<IAppRunner>().Object))
            .Object;

        private void Impossible() => throw new Exception("impossible");

        public interface IAppRunner
        {
            void Run();
        }

        private class SqlContext
        {

        }

        private class JsonContext
        {

        }
    }
}
