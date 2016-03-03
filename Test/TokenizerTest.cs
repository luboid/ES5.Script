using ES5.Script.EcmaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test
{
    public class TokenizerTest
    {
        [Fact(Skip = "temporarily")]
        public void SetTextSkipsFirstWhitespace()
        {
            var lScript = "   Test";
            var lTok = new Tokenizer();
            lTok.SetData(lScript, "");
            Assert.Equal(lTok.Token, TokenKind.Identifier);
            Assert.Equal(lTok.TokenStr, "Test");
        }

        [Fact(Skip = "temporarily")]
        public void SetTextProperlyParsesTheFirstToken()
        {
            var lScript = "Test";
            var lTok = new Tokenizer();
            lTok.SetData(lScript, "");
            Assert.Equal(lTok.Token, TokenKind.Identifier);
            Assert.Equal(lTok.TokenStr, "Test");
        }

        [Fact(Skip = "temporarily")]
        public void NextMovesToTheNextToken()
        {
            var lScript = "Test Test";
            var lTok = new Tokenizer();
            lTok.SetData(lScript, "");
            Assert.Equal(lTok.Token, TokenKind.Identifier);
            Assert.Equal(lTok.TokenStr, "Test");
            lTok.Next();
            Assert.Equal(lTok.Token, TokenKind.Identifier);
            Assert.Equal(lTok.TokenStr, "Test");
            lTok.Next();
            Assert.Equal(lTok.Token, TokenKind.EOF);
        }

        [Fact(Skip = "temporarily")]
        public void NextMovesToRightColumn()
        {
            var lScript = "   Test";
            var lTok = new Tokenizer();
            lTok.SetData(lScript, "");
            Assert.Equal(lTok.Col, 4);
        }

        [Fact(Skip = "temporarily")]
        public void NextMovesToRightNewRow()
        {
            var lScript = "Test\r\nTest";
            var lTok = new Tokenizer();
            lTok.SetData(lScript, "");
            Assert.Equal(lTok.Col, 1);
            Assert.Equal(lTok.Row, 1);
            lTok.Next();
            Assert.Equal(lTok.Col, 1);
            Assert.Equal(lTok.Row, 2);
        }

        [Fact(Skip = "temporarily")]
        public void DoubleNextEndsUpAtEOF()
        {
            var lScript = "   Test";
            var lTok = new Tokenizer();
            lTok.SetData(lScript, "");
            lTok.Next();
            Assert.Equal(lTok.Token, TokenKind.EOF);
        }

        [Fact(Skip = "temporarily")]
        public void TokenizerFailsAtWrongChar()
        {
            var lFailed = false;
            var lTok = new Tokenizer();
            lTok.Error += (Tokenizer Caller, TokenizerErrorKind Kind, string Parameter) =>
              {
                  if (Kind == TokenizerErrorKind.UnknownCharacter) lFailed = true;
              };
            var lScript = ((char)1).ToString();
            lTok.SetData(lScript, "");
            Assert.Equal(lTok.Token, TokenKind.Error);
            Assert.Equal(lFailed, true);
        }
    }
}