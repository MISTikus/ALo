using AutoFixture;
using FluentAssertions;
using System;
using Xunit;

namespace ALo.Infrastructure.Tests
{
    public class InjectorTests
    {
        private readonly Injector injector;
        private readonly Fixture random;

        public InjectorTests()
        {
            this.injector = new Injector();
            this.random = new Fixture();
        }

        [Fact]
        public void Injector_Should_Return_Same_Singletone_Registered_By_Instance()
        {
            // Arrange
            var expected = new Sample();
            this.injector.Singleton(expected);

            // Action
            var actual = this.injector.Get<Sample>();

            // Assert
            actual.Should().NotBeNull();
            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public void Injector_Should_Return_Same_Singletone_Registered_By_Type()
        {
            // Arrange
            this.injector.Singleton<Sample>();

            // Action
            var expected = this.injector.Get<Sample>();
            var actual = this.injector.Get<Sample>();

            // Assert
            actual.Should().NotBeNull();
            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public void Injector_Should_Return_Same_Singletone_Every_Time()
        {
            // Arrange
            var expected = new Sample();
            this.injector.Singleton<ISample, Sample>(expected);

            // Action
            var actual = this.injector.Get<ISample>();

            // Assert
            actual.Should().NotBeNull();
            actual.Should().BeSameAs(expected);
        }

        [Fact] // ToDo: need to check that GC collecting object after scope ends
        public void Injector_Should_Return_New_Transient_Every_Time()
        {
            // Arrange
            this.injector.Transient<ISample, Sample>();
            var notExpected = this.injector.Get<ISample>();

            // Action
            var actual = this.injector.Get<ISample>();

            // Assert
            actual.Should().NotBeNull();
            actual.Should().NotBeSameAs(notExpected);
        }

        [Fact]
        public void Injector_Should_Return_Null_If_Service_Is_Not_Registered()
        {
            // Arrange
            // Action
            var actual = this.injector.Get<ISample>();

            // Assert
            actual.Should().BeNull();
        }

        [Fact]
        public void Injector_Should_Throw_ArgumentException_If_Type_Has_No_Constructor()
        {
            // Arrange
            this.injector.Transient<NoConstructors, NoConstructors>();

            // Action
            Action<Injector> action = (i) => i.Get<NoConstructors>();

            // Assert
            this.injector
                .Invoking(action)
                .Should()
                .Throw<ArgumentException>()
                .WithMessage($"Failed to construct type '{typeof(NoConstructors).Name}'");
        }

        [Fact]
        public void Injector_Should_Take_Constructors_Ordered_By_Parameters_Length()
        {
            // Arrange
            this.injector.Transient<SecondValidConstructor, SecondValidConstructor>();

            // Action
            var actual = this.injector.Get<SecondValidConstructor>();

            // Assert
            actual.ConstructorsCalled.Should().Be(1);
        }

        [Fact]
        public void Injector_Should_Skip_Constructors_With_Unregistered_Parameters()
        {
            // Arrange
            var sample = this.random.Create<string>();
            this.injector.Singleton<string, string>(sample);
            this.injector.Transient<LastValidConstructor, LastValidConstructor>();

            // Action
            var actual = this.injector.Get<LastValidConstructor>();

            // Assert
            actual.ConstructorsCalled.Should().Be(1);
            actual.Params.Should().BeEquivalentTo(new[] { sample, sample, sample, sample, sample });
        }

        [Fact]
        public void Injector_Should_Return_Null_If_All_Constructors_With_Unregistered_Parameters()
        {
            // Arrange
            this.injector.Transient<NoValidConstructors, NoValidConstructors>();

            // Action
            var actual = this.injector.Get<NoValidConstructors>();

            // Assert
            actual.Should().BeNull();
        }

        [Fact]
        public void Injector_Should_Return_Injected_Object_With_Injected_Parameters()
        {
            // Arrange
            var sample = this.random.Create<string>();
            this.injector.Singleton<string, string>(sample);
            this.injector.Transient<LastValidConstructor, LastValidConstructor>();
            this.injector.Transient<ComplecatedConstruction, ComplecatedConstruction>();

            // Action
            var actual = this.injector.Get<ComplecatedConstruction>();

            // Assert
            actual.Should().NotBeNull();
            actual.Param.Should().NotBeNull();
            actual.Param.Params.Should().BeEquivalentTo(new[] { sample, sample, sample, sample, sample });
        }

        [Theory]
        [InlineData(typeof(IInjector))]
        [InlineData(typeof(IResolver))]
        [InlineData(typeof(IRegisterer))]
        public void Injector_Should_Register_Itself_By_All_Interfaces(Type type)
        {
            // Arrange
            var injector = new Injector();

            // Action
            var result = injector.Get(type);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(injector);
        }

        [Fact]
        public void Injector_Should_Register_All_Transient_Iterfaces()
        {
            // Arrange
            var injector = new Injector();
            injector.AllImplementationsTransient<MultipleInterfacesClass>();

            // Action
            var first = injector.Get<ISample>();
            var second = injector.Get<ICloneable>();

            // Assert
            first.Should().NotBeNull();
            second.Should().NotBeNull();
            first.Should().NotBeSameAs(second);
        }

        [Fact]
        public void Injector_Should_Register_All_Singleton_Iterfaces()
        {
            // Arrange
            var injector = new Injector();
            injector.AllImplementationsSingleton<MultipleInterfacesClass>();

            // Action
            var first = injector.Get<ISample>();
            var second = injector.Get<ICloneable>();

            // Assert
            first.Should().NotBeNull();
            second.Should().NotBeNull();
            first.Should().BeSameAs(second);
        }

        #region test samples

        private interface ISample { }

        private class Sample : ISample { }

        private class NoConstructors
        {
            private NoConstructors()
            {
            }
        }

        private class SecondValidConstructor
        {
            public int ConstructorsCalled = 0;

            public SecondValidConstructor(string s) => this.ConstructorsCalled++;

            public SecondValidConstructor() => this.ConstructorsCalled++;
        }

        private class LastValidConstructor
        {
            public int ConstructorsCalled = 0;
            public object[] Params;

            public LastValidConstructor(SecondValidConstructor s) => this.ConstructorsCalled++;

            public LastValidConstructor(NoConstructors s, SecondValidConstructor x) => this.ConstructorsCalled++;

            public LastValidConstructor(Sample s, NoConstructors x, DateTime d) => this.ConstructorsCalled++;

            public LastValidConstructor(ISample s, NoConstructors x) => this.ConstructorsCalled++;

            public LastValidConstructor(string fst, string scd, string thd, string frt, string fth)
            {
                this.ConstructorsCalled++;
                this.Params = new[] { fst, scd, thd, frt, fth };
            }
        }

        private class NoValidConstructors
        {
            public NoValidConstructors(string s)
            {
            }

            public NoValidConstructors(int i)
            {
            }

            public NoValidConstructors(DateTime d)
            {
            }
        }

        private class ComplecatedConstruction
        {
            public LastValidConstructor Param { get; set; }

            public ComplecatedConstruction(LastValidConstructor prm) => Param = prm;
        }

        private class MultipleInterfacesClass : ISample, ICloneable
        {
            public object Clone() => throw new NotImplementedException();
        }

        #endregion test samples
    }
}
