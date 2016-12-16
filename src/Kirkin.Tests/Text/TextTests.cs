using Kirkin.Text;

using Xunit;

namespace Kirkin.Tests.Text
{
    public class TextTests
    {
        [Fact]
        public void CapitalizeFirstLetterOfEachWord()
        {
            for (var i = 0; i < 100000; i++)
            {
                var text = "SOMEONE O'SOMEONE";

                Assert.Equal("Someone O'Someone", TextUtil.CapitalizeFirstLetterOfEachWord(text));

                // The below never really worked to detect a mutated
                // string because the literal itself was interned and
                // therefore mutatad by CapitalizeFirstLetterOfEachWord.
                //Assert.Equal("DANNY O'BRIEN", text);

                // Misc cases.
                Assert.Equal(string.Empty, TextUtil.CapitalizeFirstLetterOfEachWord(string.Empty));
                Assert.Equal("A", TextUtil.CapitalizeFirstLetterOfEachWord("a"));
                Assert.Equal("A", TextUtil.CapitalizeFirstLetterOfEachWord("A"));
                Assert.Equal("Zzz", TextUtil.CapitalizeFirstLetterOfEachWord("zzZ"));
            }
        }
    }
}