using ALo.Infrastructure.Pipelines;
using FluentAssertions;
using Xunit;

namespace ALo.Infrastructure.Tests.Pipelines
{
    public class DoTests
    {
        [Fact]
        public void Do_Should_Return_Same_Object()
        {
            var some = new object();
            var other = some.Do(s => { });
            other.Should().BeSameAs(some);
        }
    }
}
