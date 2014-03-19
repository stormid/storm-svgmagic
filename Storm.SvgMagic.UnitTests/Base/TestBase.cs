using Moq;
using NUnit.Framework;

namespace Storm.SvgMagic.UnitTests.Base
{
    public abstract class TestBase
    {
        [SetUp]
        public virtual void SetUp() { }

        [TearDown]
        public virtual void TearDown() { }

        protected virtual Mock<T> CreateDependency<T>() where T : class
        {
            return new Mock<T>();
        }

        protected virtual Mock<T> CreateDependency<T>(params object[] args) where T : class
        {
            return new Mock<T>(args);
        }
    }
}