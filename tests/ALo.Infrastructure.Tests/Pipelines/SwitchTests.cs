using ALo.Infrastructure.Pipelines;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace ALo.Infrastructure.Tests.Pipelines
{
    public class SwitchTests
    {
        [Fact]
        public void Switch_Should_Throw_If_Douplicate_Keys_Passed()
        {
            var some = new object();

            static void action(object s) => s.Switch("some", ("some", o => { }), ("some", o => { }));

            some.Invoking(action)
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("*some*");
        }

        [Fact]
        public void Switch_Should_Call_Action_By_Correct_Key()
        {
            var expected = "some";
            var some = new Mock<ICanIdentifyMyself>();
            some.SetupGet(s => s.WhoAmI).Returns(expected);

            some.Object
                .Switch(expected,
                    ("other", s => s.WhoAmI.Should().Be("other")),
                    (expected, s => s.WhoAmI.Should().Be(expected)),
                    ("oneMore", s => s.WhoAmI.Should().Be("oneMore"))
                );
        }

        public interface ICanIdentifyMyself
        {
            string WhoAmI { get; set; }
        }
    }
}
