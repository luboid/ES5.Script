using ES5.Script.EcmaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class Tokenizer_DecodeString
    {
        [Fact(Skip = "temporarily")]
        public void TestDecodeCharWithChr0InIt_ReturnsChr0WithoutQuotes()
        {
            Assert.Equal(Tokenizer.DecodeString("'\0'"), ((char)0).ToString());
        }

        [Fact(Skip = "temporarily")]
        public void TestDecodeSimpleString_ReturnsWithoutQuotes()
        {
            Assert.Equal(Tokenizer.DecodeString(@"""User Name"""), "User Name");
        }

        [Fact(Skip = "temporarily")]
        public void TestDecodeStringWithEscapedQuote_ReturnsStringWithOneQuote()
        {
            Assert.Equal(Tokenizer.DecodeString(@"""My \""String"""), "My \"String");
        }

        [Fact(Skip = "temporarily")]
        public void TestDecodeStringWithEscapedAsciiChar_ReturnsAsciiChar()
        {
            Assert.Equal(Tokenizer.DecodeString(@"""\xbb"""), ((char)0xbb).ToString());
        }

        [Fact(Skip = "temporarily")]
        public void TestDecodeStringWithEscapedUnicodeChar_ReturnsUnicodeChar()
        {
            Assert.Equal(Tokenizer.DecodeString(@"""\ubbbb"""), ((char)0xbbbb).ToString());
        }

        [Fact(Skip = "temporarily")]
        public void TestDecodeStringWithChr0InIt_ReturnsChr0WithoutQuotes()
        {
            var t = new String(new char[] { '"', (char)0, '"' });
            Assert.Equal(Tokenizer.DecodeString(t), ((char)0).ToString());
        }

        [Fact(Skip = "temporarily")]
        public void TestDecodeSimpleChar_ReturnsWithoutQuotes()
        {
            var t = new String(new[] { '\x27', 'c', '\x27' });
            Assert.Equal(Tokenizer.DecodeString(t), "c");
        }

        [Fact(Skip = "temporarily")]
        public void TestDecideSimpleCharWithEscapedSingleQuote_ReturnsSingleQuote()
        {
            Assert.Equal(Tokenizer.DecodeString("\x27\\\x27\x27"), ((char)39).ToString());
        }

        [Fact(Skip = "temporarily")]
        public void TestDecodeCharWithEscapedAsciiChar_ReturnsAsciiChar()
        {
            Assert.Equal(Tokenizer.DecodeString("'\\xbb'"), ((char)0xbb).ToString());
        }

        [Fact(Skip = "temporarily")]
        public void TestDecodeCharWithEscapedUnicodeChar_ReturnsUnicodeChar()
        {
            Assert.Equal(Tokenizer.DecodeString("'\\ubbbb'"), ((char)0xbbbb).ToString());
        }
    }
}