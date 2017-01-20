using Kirkin.Functional;

using NUnit.Framework;

namespace Kirkin.Tests.Functional
{
    public class HFuncTests
    {
        [Test]
        public void AdvancedCurrying()
        {
            HFunc<object, object, string> concat = s1 => s2 => $"{s1}{s2}";
            HFunc<string, int, string> twoParam = prefix => num => concat(prefix)(num);
            HFunc<string, int, string, string> threeParam = prefix => num => concat(twoParam(prefix)(num)); // suffix => concat(...)(suffix) implied.
            HFunc<int, string, string> asd = threeParam("asd"); // Partial application: prefix always "asd".

            string res1 = asd(123)("qwe"); // asd123qwe.

            HFunc<string, int, string> withoutSuffix = firstString => num => threeParam(firstString)(num)("");

            string res2 = withoutSuffix(res1)(321); // asd123qwe321.

            Assert.AreEqual("asd123qwe321", res2);
        }
    }
}