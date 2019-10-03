using ALo.Infrastructure.Pipelines;
using Moq;
using Xunit;

namespace ALo.Infrastructure.Tests.Pipelines
{
    public class IfTests
    {
        [Fact]
        public void If_Should_Execute_Action_If_Condition_Is_True()
        {
            var some = new Mock<ISome>();
            some.Object.If(true, s => s.DoSome());
            some.Verify(s => s.DoSome(), Times.Once);
        }

        [Fact]
        public void If_Should_Execute_Action_If_Condition_Function_Returns_True()
        {
            var some = new Mock<ISome>();
            some.Object.If(s => true, s => s.DoSome());
            some.Verify(s => s.DoSome(), Times.Once);
        }


        public interface ISome
        {
            void DoSome();
        }
    }
}
