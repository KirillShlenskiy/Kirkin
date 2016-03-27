using System.Reflection;

using Kirkin.Reflection;

using Xunit;

namespace Kirkin.Tests.Reflection
{
    public class MemberInfoEqualityComparerTests
    {
        [Fact]
        public void FieldEquals()
        {
            FieldInfo f1 = typeof(BaseDummy).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo f2 = typeof(BaseDummy).GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.True(f1 == f2);
            Assert.True(MemberInfoEqualityComparer.Instance.Equals(f1, f2));
        }

        class BaseDummy
        {
            private int _id = 0;

            public int ID
            {
                get
                {
                    return _id;
                }
            }
        }

        class DerivedDummy : BaseDummy
        {

        }
    }
}