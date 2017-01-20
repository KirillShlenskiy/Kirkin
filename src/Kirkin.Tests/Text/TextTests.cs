using Kirkin.Text;

using NUnit.Framework;

namespace Kirkin.Tests.Text
{
    public class TextTests
    {
        [Test]
        public void CapitalizeFirstLetterOfEachWord()
        {
            for (var i = 0; i < 100000; i++)
            {
                var text = "SOMEONE O'SOMEONE";

                Assert.AreEqual("Someone O'Someone", TextUtil.CapitalizeFirstLetterOfEachWord(text));

                // The below never really worked to detect a mutated
                // string because the literal itself was interned and
                // therefore mutatad by CapitalizeFirstLetterOfEachWord.
                //Assert.AreEqual("DANNY O'BRIEN", text);

                // Misc cases.
                Assert.AreEqual(string.Empty, TextUtil.CapitalizeFirstLetterOfEachWord(string.Empty));
                Assert.AreEqual("A", TextUtil.CapitalizeFirstLetterOfEachWord("a"));
                Assert.AreEqual("A", TextUtil.CapitalizeFirstLetterOfEachWord("A"));
                Assert.AreEqual("Zzz", TextUtil.CapitalizeFirstLetterOfEachWord("zzZ"));
            }
        }
    }
}