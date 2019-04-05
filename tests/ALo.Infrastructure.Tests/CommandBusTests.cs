using AutoFixture;
using FluentAssertions;
using Moq;
using System;

namespace ALo.Infrastructure.Tests
{
    public class CommandBusTests
    {
        private readonly Fixture random;

        public CommandBusTests() => this.random = new Fixture();

        [Xunit.Fact]
        public void CommandBus_Shuld_Send_Command_To_Registered_Handler()
        {
            // Arrange
            SampleCommand actual = null;
            var handler = new Mock<ICommandHander<SampleCommand>>();
            handler.Setup(x => x.Handle(It.IsAny<SampleCommand>())).Callback<SampleCommand>(c => actual = c);

            var resolver = new Mock<IResolver>();
            resolver.Setup(x => x.Get(typeof(ICommandHander<SampleCommand>))).Returns(handler.Object);

            var expected = this.random.Create<SampleCommand>();
            var bus = new CommandBus(resolver.Object);

            // Action
            bus.Send(expected);

            // Assert
            actual.Should().NotBeNull();
            actual.Should().BeSameAs(expected);
        }

        [Xunit.Fact]
        public void CommandBus_Should_Throw_NotImplementedException_If_Handler_Not_Found()
        {
            // Arrange
            var resolver = new Mock<IResolver>();

            var command = this.random.Create<SampleCommand>();
            var bus = new CommandBus(resolver.Object);
            void action(CommandBus c) => c.Send(command);

            // Action && Assert
            bus.Invoking(action).Should()
                .Throw<NotImplementedException>()
                .WithMessage($"Handler not found for command '{command.GetType().Name}'");
        }

        public class SampleCommand : ICommand
        {
        }
    }
}
