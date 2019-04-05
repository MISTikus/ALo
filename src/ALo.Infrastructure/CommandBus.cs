using System;
using System.Diagnostics;

namespace ALo.Infrastructure
{
    public interface ICommand
    {
    }
    public interface ICommandHander<TCommand> where TCommand : ICommand
    {
        void Handle(TCommand command);
    }
    public interface ICommandBus
    {
        void Send(ICommand command);
    }
    public class CommandBus : ICommandBus
    {
        private readonly IResolver resolver;

        public CommandBus(IResolver resolver) => this.resolver = resolver;

        public void Send(ICommand command)
        {
            var type = command.GetType();
            Trace.WriteLine($"Sending command '{type.Name}'");

            var handlerInterface = typeof(ICommandHander<>);
            var generic = handlerInterface.MakeGenericType(type);
            var handle = generic.GetMethod(nameof(ICommandHander<ICommand>.Handle));

            var handler = this.resolver.Get(generic);
            if (handler == null)
                throw new NotImplementedException($"Handler not found for command '{type.Name}'");
            handle.Invoke(handler, new[] { command });
        }
    }
}
