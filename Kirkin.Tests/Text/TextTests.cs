using Kirkin.Text;

using Xunit;

namespace Kirkin.Tests.Text
{
    public class TextTests
    {
        [Fact]
        public void CapitaliseFirstLetterOfEachWord()
        {
            for (var i = 0; i < 100000; i++)
            {
                var text = "SOMEONE O'SOMEONE";

                Assert.Equal("Someone O'Someone", TextUtil.CapitaliseFirstLetterOfEachWord(text));

                // The below never really worked to detect a mutated
                // string because the literal itself was interned and
                // therefore mutatad by CapitaliseFirstLetterOfEachWord.
                //Assert.Equal("DANNY O'BRIEN", text);

                // Misc cases.
                Assert.Equal(string.Empty, TextUtil.CapitaliseFirstLetterOfEachWord(string.Empty));
                Assert.Equal("A", TextUtil.CapitaliseFirstLetterOfEachWord("a"));
                Assert.Equal("A", TextUtil.CapitaliseFirstLetterOfEachWord("A"));
                Assert.Equal("Zzz", TextUtil.CapitaliseFirstLetterOfEachWord("zzZ"));
            }
        }
    }
}