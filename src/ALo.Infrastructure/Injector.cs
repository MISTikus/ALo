using System;
using System.Collections.Generic;
using System.Linq;

namespace ALo.Infrastructure
{
    public interface IInjector : IResolver, IRegisterer { }
    public interface IResolver
    {
        TService Get<TService>();

        object Get(Type type);
    }
    public interface IRegisterer
    {
        void AllImplementationsSingleton<TImplementation>(TImplementation instance = null) where TImplementation : class;

        void AllImplementationsTransient<TImplementation>() where TImplementation : class;

        void Singleton<TImplementation>(TImplementation instance = null) where TImplementation : class;

        void Singleton<TService, TImplementation>(TImplementation instance = null) where TImplementation : class, TService;

        void Transient<TImplementation>() where TImplementation : class;

        void Transient<TService, TImplementation>() where TImplementation : class, TService;
    }

    public class Injector : IInjector
    {
        private readonly Dictionary<Type, object> singletons;
        private readonly Dictionary<Type, Type> singletonTypes;
        private readonly Dictionary<Type, Type> transients;

        // ToDo: Build dependency graph while registering into expression
        public Injector()
        {
            this.singletons = new Dictionary<Type, object>();
            this.singletonTypes = new Dictionary<Type, Type>();
            this.transients = new Dictionary<Type, Type>();

            AllImplementationsSingleton(this);
        }

        public void AllImplementationsSingleton<TImplementation>(TImplementation instance = null) where TImplementation : class
        {
            var type = typeof(TImplementation);
            var interfaces = type.GetInterfaces();
            if (instance == null)
            {
                foreach (var @interface in interfaces)
                    this.singletonTypes.Add(@interface, type);
            }
            else
            {
                foreach (var @interface in interfaces)
                    this.singletons.Add(@interface, instance);
            }
        }

        public void AllImplementationsTransient<TImplementation>() where TImplementation : class
        {
            var type = typeof(TImplementation);
            var interfaces = type.GetInterfaces();
            foreach (var @interface in interfaces)
                this.transients.Add(@interface, type);
        }

        public void Singleton<TService, TImplementation>(TImplementation instance = null)
            where TImplementation : class, TService
        {
            if (instance == null)
                this.singletonTypes[typeof(TService)] = typeof(TImplementation);
            else
                this.singletons[typeof(TService)] = instance;
        }

        public void Singleton<TImplementation>(TImplementation instance = null) where TImplementation : class
        {
            if (instance == null)
                this.singletonTypes[typeof(TImplementation)] = typeof(TImplementation);
            else
                this.singletons[typeof(TImplementation)] = instance;
        }

        public void Transient<TImplementation>() where TImplementation : class =>
            this.transients[typeof(TImplementation)] = typeof(TImplementation);

        public void Transient<TService, TImplementation>()
            where TImplementation : class, TService =>
            this.transients[typeof(TService)] = typeof(TImplementation);

        public TService Get<TService>() => (TService)Get(typeof(TService));

        public object Get(Type type)
        {
            if (this.singletons.ContainsKey(type))
                return this.singletons[type];
            if (this.singletonTypes.ContainsKey(type))
            {
                var obj = Construct(this.singletonTypes[type]);
                if (obj != null)
                {
                    var singletons = this.singletonTypes.Where(x => x.Value == this.singletonTypes[type]).ToArray();
                    foreach (var singleton in singletons)
                    {
                        this.singletons[singleton.Key] = obj;
                        this.singletonTypes.Remove(singleton.Key);
                    }
                    return obj;
                }
            }
            if (this.transients.ContainsKey(type))
                return Construct(this.transients[type]);
            return null;
        }

        private object Construct(Type type)
        {
            var constructors = type.GetConstructors().OrderBy(c => c.GetParameters().Length);
            if (!constructors.Any() && type.IsClass)
                throw new ArgumentException($"Failed to construct type '{type.Name}'");
            else if (!type.IsClass) // ToDo: test it!
                return Activator.CreateInstance(type);

            foreach (var ctor in constructors)
            {
                var parameters = ctor.GetParameters();
                if (parameters.Length == 0)
                    return Activator.CreateInstance(type);

                if (parameters.Any(p =>
                    !this.singletons.ContainsKey(p.ParameterType)
                    && !this.singletonTypes.ContainsKey(p.ParameterType)
                    && !this.transients.ContainsKey(p.ParameterType)
                ))
                    continue;

                var parameterInstances = new List<object>();
                foreach (var parameter in parameters)
                {
                    parameterInstances.Add(Get(parameter.ParameterType));
                }
                return Activator.CreateInstance(type, parameterInstances.ToArray());
            }
            return null;
        }
    }
}
