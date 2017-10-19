using Frends.Radon.LiquidExtensions;
using NUnit.Framework;

namespace Frends.Radon.Tests.LiquidExtensions
{
    [TestFixture]
    public class FilterExtensionsTest
    {
        [Test]
        public void MatchFirst_ShouldRegexMatchFirstWithCaptureGroup()
        {
            Assert.That(FilterExtensions.MatchFirst("Foo: Shakalaka\nFoo: ShakalakaBar", "(?m)^Foo: (.+)$"), Is.EqualTo("Shakalaka"));
        }

        [Test]
        public void MatchFirst_ShouldRegexMatchFirstWithoutCaptureGroup()
        {
            Assert.That(FilterExtensions.MatchFirst("Foo: Shakalaka\nFoo: ShakalakaBar", "(?m)^Foo: .+$"), Is.EqualTo("Foo: Shakalaka"));
        }

        [Test]
        public void MatchFirst_ShouldReturnEmptyStringIfNoMatch()
        {
            Assert.That(FilterExtensions.MatchFirst("Foo: Shakalaka\nFoo: ShakalakaBar", "(?m)^Foos: .+$"), Is.EqualTo(""));
        }
    }
}
