using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class Tokenizer : ITokenizer
    {
        class KeywordMap
        {
            public KeywordMap(string aOriginal, TokenKind aToken)
            {
                Token = aToken;
                Original = aOriginal;
                Chars = aOriginal.ToCharArray();
            }

            public readonly char[] Chars;
            public readonly TokenKind Token;
            public readonly string Original;
        }

        static KeywordMap[] fIdentifiers = null;

        static Tokenizer()
        {
            List<KeywordMap> lItems = new List<Tokenizer.KeywordMap>();

            lItems.Add(new KeywordMap("break", TokenKind.K_break));
            lItems.Add(new KeywordMap("do", TokenKind.K_do));
            lItems.Add(new KeywordMap("instanceof", TokenKind.K_instanceof));
            lItems.Add(new KeywordMap("typeof", TokenKind.K_typeof));
            lItems.Add(new KeywordMap("case", TokenKind.K_case));
            lItems.Add(new KeywordMap("else", TokenKind.K_else));
            lItems.Add(new KeywordMap("new", TokenKind.K_new));
            lItems.Add(new KeywordMap("var", TokenKind.K_var));
            lItems.Add(new KeywordMap("catch", TokenKind.K_catch));
            lItems.Add(new KeywordMap("finally", TokenKind.K_finally));
            lItems.Add(new KeywordMap("return", TokenKind.K_return));
            lItems.Add(new KeywordMap("void", TokenKind.K_void));
            lItems.Add(new KeywordMap("continue", TokenKind.K_continue));
            lItems.Add(new KeywordMap("for", TokenKind.K_for));
            lItems.Add(new KeywordMap("switch", TokenKind.K_switch));
            lItems.Add(new KeywordMap("while", TokenKind.K_while));
            lItems.Add(new KeywordMap("debugger", TokenKind.K_debugger));
            lItems.Add(new KeywordMap("function", TokenKind.K_function));
            lItems.Add(new KeywordMap("this", TokenKind.K_this));
            lItems.Add(new KeywordMap("with", TokenKind.K_with));
            lItems.Add(new KeywordMap("default", TokenKind.K_default));
            lItems.Add(new KeywordMap("if", TokenKind.K_if));
            lItems.Add(new KeywordMap("throw", TokenKind.K_throw));
            lItems.Add(new KeywordMap("delete", TokenKind.K_delete));
            lItems.Add(new KeywordMap("in", TokenKind.K_in));
            lItems.Add(new KeywordMap("try", TokenKind.K_try));
            lItems.Add(new KeywordMap("null", TokenKind.K_null));
            lItems.Add(new KeywordMap("true", TokenKind.K_true));
            lItems.Add(new KeywordMap("false", TokenKind.K_false));
            lItems.Add(new KeywordMap("get", TokenKind.K_get));
            lItems.Add(new KeywordMap("set", TokenKind.K_set));

            lItems.Sort((a, b) => a.Original.CompareTo(b.Original));
            fIdentifiers = lItems.ToArray();
        }

        char[] fInput;
        int fPos;
        int fRow;
        int fLastEnterPos;
        int fLen;
        TokenKind fToken;
        char[] fTokenStr;
        Position fPosition = new Position();
        Position fEndPosition = new Position();
        Position fLastEndPosition = new Position();
        bool fLastWasEnter;
        bool fJSON = false;

        public bool LastWasEnter
        {
            get
            {
                return fLastWasEnter;
            }
        }

        public int Pos { get { return fPos; } }
        public int Row { get { return fRow; } }
        public int Col { get { return fPos - fLastEnterPos; } }
        public TokenKind Token { get { return fToken; } }
        public string TokenStr { get { return fTokenStr == null ? String.Empty : new String(fTokenStr); } }
        public Position Position { get { return fPosition; } }
        public Position EndPosition { get { return fEndPosition; } }
        public Position LastEndPosition
        {
            get { return fLastEndPosition; }
        }

        public PositionPair PositionPair
        {
            get { return new PositionPair(Position, EndPosition); }
        }

        public bool JSON { get { return fJSON; } set { fJSON = value; } }


        public event TokenizerError Error;


        int IdentCompare(int aPos, int len, char[] data)
        {
            for (int i = 0, l = (len > data.Length ? data.Length : len); i < l; i++)
            {
                if (data[i] > fInput[aPos + i]) return 1;
                if (data[i] < fInput[aPos + i]) return -1;
            }
            if (len < data.Length) return 1;
            if (len > data.Length) return -1;
            return 0;
        }

        TokenKind IsIdentifier(int aPos, int len)
        {
            var L = 0;
            var H = fIdentifiers.Length - 1;
            while (L <= H)
            {
                var curr = (L + H) / 2;
                switch (IdentCompare(aPos, len, fIdentifiers[curr].Chars))
                {
                    case 0:
                        return fIdentifiers[curr].Token;
                    case 1:
                        H = curr - 1;
                        break;
                    case -1:
                        L = curr + 1;
                        break;
                }
            }
            return TokenKind.Identifier;
        }

        public void SetData(string input, string filename)
        {
            fInput = (input + "\x0\x0\x0\x0").ToCharArray();
            fLen = 0;
            fPos = 0;
            fRow = 1;
            fLastEnterPos = -1;
            fEndPosition.Pos = 0;
            fEndPosition.Col = 1;
            fEndPosition.Row = 1;

            fPosition.Module = filename;
            Next();
        }

        public void Next()
        {
            var Stop = false;
            fLastWasEnter = false;
            fLastEndPosition = EndPosition;

            while (!Stop)
            {
                fPos = fPos + fLen;
                var lStartRow = Row; // Actual row
                var lStartColumn = Col;
                if (!IntNext())
                {
                    fToken = TokenKind.Error;
                    return;
                }
                switch (fToken)
                {
                    case TokenKind.LineFeed:
                    case TokenKind.LineComment:
                    case TokenKind.MultilineComment:
                        fLastWasEnter = true;
                        break;
                    case TokenKind.Comment:
                    case TokenKind.Whitespace: break;
                    default:
                        Stop = true;
                        fPosition.Col = lStartColumn;
                        fPosition.Row = lStartRow;
                        fPosition.Pos = fPos;
                        fEndPosition.Col = (fPos + fLen) - fLastEnterPos;
                        fEndPosition.Row = Row;
                        fEndPosition.Pos = fPos + fLen;
                        break;
                }
            }
        }

        public static string DecodeString(string aString)
        {
            var lRes = new System.Text.StringBuilder(aString.Length);
            var i = 1;
            var l = aString.Length - 1;
            while (i < l)
            {
                switch (aString[i])
                {
                    case '\\':
                        {
                            i++;
                            switch (aString[i])
                            {
                                case 'x':
                                    {
                                        lRes.Append((char)Int32.Parse(aString.Substring(i + 1, 2), System.Globalization.NumberStyles.HexNumber));
                                        i += 2;
                                    }
                                    break;
                                case 'u':
                                    {
                                        lRes.Append((char)Int32.Parse(aString.Substring(i + 1, 4), System.Globalization.NumberStyles.HexNumber));
                                        i += 4;
                                    }
                                    break;
                                case 'b': lRes.Append('\x8'); break;
                                case 't': lRes.Append('\x9'); break;
                                case 'n': lRes.Append('\xA'); break;
                                case 'r': lRes.Append('\xD'); break;
                                case 'v': lRes.Append('\xB'); break;
                                case 'f': lRes.Append('\xC'); break;
                                case '"': lRes.Append('"'); break;
                                case '\x27': lRes.Append('\x27'); break;
                                case '\\': lRes.Append('\\'); break;
                                case '\x0': lRes.Append('\x0'); break;
                                case '\xA':
                                    // do nothing
                                    break;
                                case '\xD':
                                    {
                                        if (aString[i + 1] == '\xA') ++i;
                                    }
                                    break;
                                default:
                                    lRes.Append(aString[i]);
                                    break;
                            } // case
                            ++i;
                        }
                        break;
                    default:
                        lRes.Append(aString[i]);
                        ++i;
                        break;
                } // case
            }
            return lRes.ToString();
        }

        public object SaveState()
        {
            var lResult = new Object[8];
            lResult[0] = fPos;
            lResult[1] = fRow;
            lResult[2] = fLastEnterPos;
            lResult[3] = fLen;
            lResult[4] = fToken;
            lResult[5] = fTokenStr;
            lResult[6] = fPosition;
            lResult[7] = fLastWasEnter;
            return lResult;
        }

        public void RestoreState(object o)
        {
            var lArr = (object[])o;
            fPos = (int)lArr[0];
            fRow = (int)lArr[1];
            fLastEnterPos = (int)lArr[2];
            fLen = (int)lArr[3];
            fToken = (TokenKind)lArr[4];
            fTokenStr = (char[])lArr[5];
            fPosition = (ES5.Script.Position)lArr[6];
            fLastWasEnter = (bool)lArr[7];
        }

        public void NextAsRegularExpression(out string aString, out string aModifiers)
        {
            aModifiers = null;
            aString = null;
            fPos = fPos + fLen; // should be 1 at this point, the / 
            var curroffset = fPos;
            while (fInput[curroffset] != '/')
            {
                if (Utilities.InSet(fInput[curroffset], '\xD', '\x10', '\x2028', '\x2029'))
                {
                    if (Error != null) Error(this, TokenizerErrorKind.EnterInRegex, "");
                    fLen = curroffset - fPos + 1;
                    fToken = TokenKind.Error;
                    return;
                }

                if ((fInput[curroffset] == '\x0') && (curroffset >= fInput.Length - 4))
                {
                    if (Error != null) Error(this, TokenizerErrorKind.EOFInRegex, "");
                    fLen = curroffset - fPos + 1;
                    fToken = TokenKind.Error;
                    return;
                }

                if (fInput[curroffset] == '\\')
                {
                    ++curroffset;
                    if (Utilities.InSet(fInput[curroffset], '\xD', '\x10', '\x2028', '\x2029'))
                    {
                        if (Error != null) Error(this, TokenizerErrorKind.EnterInRegex, "");
                        fLen = curroffset - fPos + 1;
                        fToken = TokenKind.Error;
                        return;
                    }
                }
                ++curroffset;
            }
            aString = new String(fInput, fPos, curroffset - fPos);
            ++curroffset;
            fPos = curroffset;
            fLen = 0;
            Next();
            if (fToken == TokenKind.Identifier)
            {
                aModifiers = TokenStr;
                Next();
            }
            else
            {
                aModifiers = "";
            }
        }

        bool IntNext()
        {
            var curroffset = fPos;
            var lHadUnicodeIdentifier = 0;

            if (Utilities.InSets2(fInput[curroffset], 'A', 'Z', 'a', 'z', '$', '_') ||
              ((fInput[curroffset] > 127) && (!Utilities.InSets1(fInput[curroffset], 0x20000, 0x200B, 133, 160, 0x200B, 0xFEFF, 0x202f, 0x205f, 0x3000, 0x2028, 0x2029)))
              || ((fInput[curroffset] == '\\') && (fInput[curroffset + 1] == 'u' || fInput[curroffset + 1] == 'U') &&
                Utilities.InSets3(fInput[curroffset + 2], '0', '9', 'a', 'f', 'A', 'F') &&
                Utilities.InSets3(fInput[curroffset + 3], '0', '9', 'a', 'f', 'A', 'F') &&
                Utilities.InSets3(fInput[curroffset + 4], '0', '9', 'a', 'f', 'A', 'F') &&
                Utilities.InSets3(fInput[curroffset + 5], '0', '9', 'a', 'f', 'A', 'F'))) {
                if (fInput[curroffset] == '\\') {
                    curroffset += 5;
                    ++lHadUnicodeIdentifier;
                }
                ++curroffset;

                while (Utilities.InSets3(fInput[curroffset], 'A', 'Z', 'a', 'z', '0', '9', '_', '$') ||
                  ((fInput[curroffset] > 127) && !Utilities.InSets1(fInput[curroffset], 0x2000, 0x200B, 133, 160, 0x200B, 0xFEFF, 0x202f, 0x205f, 0x3000, 0x2028, 0x2029)) ||
                  ((fInput[curroffset] == '\\') && (fInput[curroffset + 1] == 'u' || fInput[curroffset + 1] == 'U') &&
                   Utilities.InSets3(fInput[curroffset + 2], '0', '9', 'a', 'f', 'A', 'F') &&
                   Utilities.InSets3(fInput[curroffset + 3], '0', '9', 'a', 'f', 'A', 'F') &&
                   Utilities.InSets3(fInput[curroffset + 4], '0', '9', 'a', 'f', 'A', 'F') &&
                   Utilities.InSets3(fInput[curroffset + 5], '0', '9', 'a', 'f', 'A', 'F'))) {
                    if (fInput[curroffset] == '\\') {
                        curroffset += 5;
                        ++lHadUnicodeIdentifier;
                    }
                    ++curroffset;
                }
                fLen = curroffset - fPos;
                if (lHadUnicodeIdentifier > 0) {
                    fToken = TokenKind.Identifier;
                    fTokenStr = new Char[fLen - (5 * lHadUnicodeIdentifier)];
                    lHadUnicodeIdentifier = 0;
                    var i = 0;
                    while (i < fLen) {
                        if (fInput[fPos + i] == '\\') {
                            fTokenStr[lHadUnicodeIdentifier] = (Char)UInt16.Parse(String.Concat(fInput[fPos + i + 2], fInput[fPos + i + 3], fInput[fPos + i + 4], fInput[fPos + i + 5]), System.Globalization.NumberStyles.HexNumber);
                            ++lHadUnicodeIdentifier;
                            i += 6;
                        } else {
                            fTokenStr[lHadUnicodeIdentifier] = fInput[fPos + i];
                            ++lHadUnicodeIdentifier;
                            ++i;
                        }
                    }
                } else {
                    fToken = IsIdentifier(fPos, fLen);
                    if (fToken == TokenKind.K_get || fToken == TokenKind.K_set || fToken == TokenKind.Identifier)
                    {
                        fTokenStr = new Char[fLen];
                        Array.Copy(fInput, fPos, fTokenStr, 0, fLen);
                    }
                }
            } else { // end of "identifier"
                fTokenStr = null;
                switch ((int)fInput[curroffset]) {
                    case '/':
                        {
                            if (fInput[curroffset + 1] == '/') { // single line commment
                                ++curroffset;
                                while (!Utilities.InSet(fInput[curroffset], 10, 13, 0x2028, 0x2029) && !((fInput[curroffset] == 0) && (curroffset >= fInput.Length - 4)))
                                    ++curroffset;

                                fLen = curroffset - fPos;
                                fToken = TokenKind.LineComment;
                            } else if (fInput[curroffset + 1] == '*') {
                                curroffset = curroffset + 2;
                                var lHadEnters = false;
                                while (!((fInput[curroffset] == '*') && (fInput[curroffset + 1] == '/'))) {
                                    if (Utilities.InSet(fInput[curroffset], 13, 10, 0x2028, 0x2029)) {
                                        if ((fInput[curroffset + 1] == 10) && (fInput[curroffset] == 13)) ++curroffset;
                                        fLastEnterPos = curroffset;
                                        lHadEnters = true;
                                        ++fRow;
                                    }
                                    if ((fInput[curroffset] == 0) && (curroffset >= fInput.Length - 4)) break;
                                    ++curroffset;
                                }

                                if (fInput[curroffset] == 0) {
                                    if (Error != null) Error(this, TokenizerErrorKind.CommentError, "");
                                    fLen = 0;
                                    return false;
                                } else {
                                    fLen = curroffset - fPos + 2;
                                    if (lHadEnters)
                                        fToken = TokenKind.MultilineComment;
                                    else
                                        fToken = TokenKind.Comment;
                                }
                            } else if (fInput[curroffset + 1] == '=') {
                                fLen = 2;
                                fToken = TokenKind.DivideAssign;
                            } else {
                                fLen = 1;
                                fToken = TokenKind.Divide;
                            }
                        }
                        break;
                    case 13:
                    case 10:
                    case 0x2028:
                    case 0x2029:
                        {
                            if (Utilities.InSet(fInput[curroffset], 13, 10, 0x2028, 0x2029)) {
                                if ((fInput[curroffset + 1] == 10) && (fInput[curroffset] == 13)) ++curroffset;
                                fLastEnterPos = curroffset;
                                ++fRow;
                            }
                            ++curroffset;
                            fLen = curroffset - fPos;
                            fToken = TokenKind.LineFeed;
                        }
                        break;
                    case 11:
                    case 9:
                    case 12:
                    case 32:
                    case 133:
                    case 160://#$2000 .. #$200B, 
                    case 0x2000:
                    case 0x2001:
                    case 0x2002:
                    case 0x2003:
                    case 0x2004:
                    case 0x2005:
                    case 0x2006:
                    case 0x2007:
                    case 0x2008:
                    case 0x2009:
                    case 0x200A:
                    case 0x200B:
                    case 0xFEFF:
                    case 0x202f:
                    case 0x205f:
                    case 0x3000:
                        { //whitespace
                            if (fJSON && (fInput[curroffset] == 11)) {
                                fLen = 0;
                                if (Error != null) Error(this, TokenizerErrorKind.UnknownCharacter, fInput[curroffset].ToString());
                                fToken = TokenKind.Error;
                                return false;
                            }

                            ++curroffset; // 
                            while (Utilities.InSets1(fInput[curroffset], 0x2000, 0x200B, 9, 12, 11, 32, 133, 160,
                              0x200B, 0xFEFF, 0x202f, 0x205f, 0x3000)) {
                                if (fJSON && (fInput[curroffset] == 11)) {
                                    fLen = 0;
                                    if (Error != null) Error(this, TokenizerErrorKind.UnknownCharacter, fInput[curroffset].ToString());
                                    fToken = TokenKind.Error;
                                    return false;
                                }
                                ++curroffset;
                            }
                            fLen = curroffset - fPos;
                            fToken = TokenKind.Whitespace;
                        }
                        break;
                    case 0: {
                            fLen = 0;
                            fToken = TokenKind.EOF;
                        }
                        break;
                    case ':':
                        {
                            fLen = 1;
                            fToken = TokenKind.Colon;
                        }
                        break;
                    case ';':
                        {
                            fLen = 1;
                            fToken = TokenKind.Semicolon;
                        }
                        break;
                    case ',':
                        {
                            fLen = 1;
                            fToken = TokenKind.Comma;
                        }
                        break;
                    case '=':
                        {
                            if (fInput[curroffset + 1] == '=') {
                                if (fInput[curroffset + 2] == '=') {
                                    fLen = 3;
                                    fToken = TokenKind.StrictEqual;
                                }
                                else
                                {
                                    fLen = 2;
                                    fToken = TokenKind.Equal;
                                }
                            }
                            else
                            {
                                fLen = 1;
                                fToken = TokenKind.Assign;
                            }
                        }
                        break;
                    case '!':
                        {
                            if (fInput[curroffset + 1] == '=') {
                                if (fInput[curroffset + 2] == '=') {
                                    fLen = 3;
                                    fToken = TokenKind.StrictNotEqual;
                                } else {
                                    fLen = 2;
                                    fToken = TokenKind.NotEqual;
                                }
                            }
                            else
                            {
                                fLen = 1;
                                fToken = TokenKind.Not;
                            }
                        }
                        break;
                    case '<':
                        {
                            if (fInput[curroffset + 1] == '<') {
                                if (fInput[curroffset + 2] == '=') {
                                    fLen = 3;
                                    fToken = TokenKind.ShiftLeftAssign;
                                } else
                                {
                                    fLen = 2;
                                    fToken = TokenKind.ShiftLeft;
                                }
                            } else
                            if (fInput[curroffset + 1] == '=') {
                                fLen = 2;
                                fToken = TokenKind.LessOrEqual;
                            } else {
                                fLen = 1;
                                fToken = TokenKind.Less;
                            }
                        }
                        break;
                    case '>':
                        {
                            if (fInput[curroffset + 1] == '>') {
                                if (fInput[curroffset + 2] == '>') {
                                    if (fInput[curroffset + 3] == '=') {
                                        fLen = 4;
                                        fToken = TokenKind.ShiftRightUnsignedAssign;
                                    }
                                    else {
                                        fLen = 3;
                                        fToken = TokenKind.ShiftRightUnsigned;
                                    }
                                } else if (fInput[curroffset + 2] == '=') {
                                    fLen = 3;
                                    fToken = TokenKind.ShiftRightSignedAssign;
                                } else {
                                    fLen = 2;
                                    fToken = TokenKind.ShiftRightSigned;
                                }
                            } else if (fInput[curroffset + 1] == '=') {
                                fLen = 2;
                                fToken = TokenKind.GreaterOrEqual;
                            } else {
                                fLen = 1;
                                fToken = TokenKind.Greater;
                            }
                        }
                        break;
                    case '*':
                        {
                            if (fInput[curroffset + 1] == '=') {
                                fLen = 2;
                                fToken = TokenKind.MultiplyAssign;
                            } else {
                                fLen = 1;
                                fToken = TokenKind.Multiply;
                            }
                        }
                        break;
                    case '{':
                        {
                            fLen = 1;
                            fToken = TokenKind.CurlyOpen;
                        }
                        break;
                    case '}':
                        {
                            fLen = 1;
                            fToken = TokenKind.CurlyClose;
                        }
                        break;
                    case '(':
                        {
                            fLen = 1;
                            fToken = TokenKind.OpeningParenthesis;
                        }
                        break;
                    case ')':
                        {
                            fLen = 1;
                            fToken = TokenKind.ClosingParenthesis;
                        }
                        break;
                    case '[':
                        {
                            fLen = 1;
                            fToken = TokenKind.OpeningBracket;
                        }
                        break;
                    case ']':
                        {
                            fLen = 1;
                            fToken = TokenKind.ClosingBracket;
                        }
                        break;
                    case '%':
                        {
                            if (fInput[curroffset + 1] == '=')
                            {
                                fLen = 2;
                                fToken = TokenKind.ModulusAssign;
                            }
                            else
                            {
                                fLen = 1;
                                fToken = TokenKind.Modulus;
                            }
                        }
                        break;
                    case '&':
                        {
                            if (fInput[curroffset + 1] == '=') {
                                fLen = 2;
                                fToken = TokenKind.AndAssign;
                            }
                            else
                            if (fInput[curroffset + 1] == '&') {
                                fLen = 2;
                                fToken = TokenKind.DoubleAnd;
                            }
                            else
                            {
                                fLen = 1;
                                fToken = TokenKind.And;
                            }
                        }
                        break;
                    case '|':
                        {
                            if (fInput[curroffset + 1] == '=')
                            {
                                fLen = 2;
                                fToken = TokenKind.OrAssign;
                            }
                            else
                            if (fInput[curroffset + 1] == '|')
                            {
                                fLen = 2;
                                fToken = TokenKind.DoubleOr;
                            }
                            else
                            {
                                fLen = 1;
                                fToken = TokenKind.Or;
                            }
                        }
                        break;
                    case '^':
                        {
                            if (fInput[curroffset + 1] == '=') {
                                fLen = 2;
                                fToken = TokenKind.XorAssign;
                            } else
                            {
                                fLen = 1;
                                fToken = TokenKind.Xor;
                            }
                        }
                        break;
                    case '-':
                        {
                            if (fInput[curroffset + 1] == '-')
                            {
                                fLen = 2;
                                fToken = TokenKind.Decrement;
                            }
                            else if (fInput[curroffset + 1] == '=')
                            {
                                fLen = 2;
                                fToken = TokenKind.MinusAssign;
                            }
                            else
                            {
                                fLen = 1;
                                fToken = TokenKind.Minus;
                            }
                        }
                        break;
                    case '~':
                        {
                            fLen = 1;
                            fToken = TokenKind.BitwiseNot;
                        }
                        break;
                    case '?':
                        {
                            fLen = 1;
                            fToken = TokenKind.ConditionalOperator;
                        }
                        break;
                    case '+':
                        {
                            if (fInput[curroffset + 1] == '+')
                            {
                                fLen = 2;
                                fToken = TokenKind.Increment;
                            }
                            else
                            if (fInput[curroffset + 1] == '=')
                            {
                                fLen = 2;
                                fToken = TokenKind.PlusAssign;
                            }
                            else
                            {
                                fLen = 1;
                                fToken = TokenKind.Plus;
                            }
                        }
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '.':
                        {
                            if ((fInput[curroffset] == '.') && (!Utilities.InSets1(fInput[curroffset + 1], '0', '9')))
                            {
                                fLen = 1;
                                fToken = TokenKind.Dot;
                            }
                            else
                            {
                                if ((fInput[curroffset] == '0') && (fInput[curroffset + 1] == 'x' || fInput[curroffset + 1] == 'X'))
                                {
                                    curroffset = curroffset + 2;
                                    while (Utilities.InSets3(fInput[curroffset], '0', '9', 'A', 'F', 'a', 'f')) ++curroffset;
                                    fToken = TokenKind.HexNumber;
                                }
                                else
                                {
                                    var lHasDot = fInput[curroffset] == '.';
                                    ++curroffset;
                                    while (Utilities.InSets1(fInput[curroffset], '0', '9') || ((fInput[curroffset] == '.') && !lHasDot))
                                    {
                                        if (fInput[curroffset] == '.') lHasDot = true;
                                        ++curroffset;
                                    }
                                    if (fInput[curroffset] == 'E' || fInput[curroffset] == 'e')
                                    {
                                        lHasDot = true;
                                        ++curroffset;
                                        if (fInput[curroffset] == '+' || fInput[curroffset] == '-') ++curroffset;
                                        while ('0' <= fInput[curroffset] && fInput[curroffset] <= '9')
                                        {
                                            ++curroffset;
                                        }
                                    }

                                    if (lHasDot)
                                        fToken = TokenKind.Float;
                                    else
                                        fToken = TokenKind.Number;
                                }
                                fLen = curroffset - fPos;
                                fTokenStr = new Char[fLen];
                                Array.Copy(fInput, fPos, fTokenStr, 0, fLen);
                            }
                        }
                        break;
                    case '"':
                        {
                            ++curroffset;
                            while ((fInput[curroffset] != 0) && (fInput[curroffset] != '"')) {

                                if (fInput[curroffset] == '\\') {
                                    ++curroffset;
                                    switch ((int)fInput[curroffset]) {
                                        case 'x':
                                            {
                                                if ((fInput[curroffset + 1] == 0) || (fInput[curroffset + 2] == 0)) {
                                                    if (Error != null) Error(this, TokenizerErrorKind.EOFInString, "");
                                                    fLen = curroffset - fPos;
                                                    return false;
                                                } else
                                                if (!Utilities.InSets3(fInput[curroffset + 1], '0', '9', 'A', 'F', 'a', 'f') ||
                                                    !Utilities.InSets3(fInput[curroffset + 2], '0', '9', 'A', 'F', 'a', 'f')) {
                                                    if (Error != null) Error(this, TokenizerErrorKind.InvalidEscapeSequence, "");
                                                    fLen = curroffset - fPos;
                                                    return false;
                                                } else
                                                    curroffset += 2;
                                            }
                                            break;
                                        case 'u':
                                            if ((fInput[curroffset + 1] == 0) || (fInput[curroffset + 2] == 0) || (fInput[curroffset + 3] == 0) ||
                                                (fInput[curroffset + 4] == 0))
                                            {
                                                if (Error != null) Error(this, TokenizerErrorKind.EOFInString, "");
                                                fLen = curroffset - fPos;
                                                return false;
                                            }
                                            else
                                            if (!Utilities.InSets3(fInput[curroffset + 1], '0', '9', 'A', 'F', 'a', 'f') ||
                                                !Utilities.InSets3(fInput[curroffset + 2], '0', '9', 'A', 'F', 'a', 'f') ||
                                                !Utilities.InSets3(fInput[curroffset + 3], '0', '9', 'A', 'F', 'a', 'f') ||
                                                !Utilities.InSets3(fInput[curroffset + 4], '0', '9', 'A', 'F', 'a', 'f'))
                                            {
                                                if (Error != null) Error(this, TokenizerErrorKind.InvalidEscapeSequence, "");
                                                fLen = curroffset - fPos;
                                                return false;
                                            }
                                            else curroffset += 4;
                                            break;
                                        case 13:
                                            if (fInput[curroffset + 1] == 10) ++curroffset;
                                            break;

                                        case 10:
                                        case '\'':
                                        case 'b':
                                        case 't':
                                        case 'n':
                                        case 'r':
                                        case 'v':
                                        case 'f':
                                        case '"':
                                        case 9:
                                        case '0':
                                        case '\\':
                                            break;
                                        default: {
                                                //if Error != nil then Error(self, TokenizerErrorKind.InvalidEscapeSequence, '');
                                                //FLen := curroffset - FPos;
                                            }
                                            break;
                                    } // case
                                }
                                else if (fJSON && (0x0 <= fInput[curroffset] && fInput[curroffset] <= 0x1f)) {
                                    if (Error != null) Error(this, TokenizerErrorKind.InvalidEscapeSequence, "");
                                    fLen = curroffset - fPos;
                                    return false;
                                }

                                ++curroffset;
                            }
                            fLen = curroffset - fPos + 1;
                            fToken = TokenKind.String;
                            fTokenStr = new Char[fLen];
                            Array.Copy(fInput, fPos, fTokenStr, 0, fLen);
                            if (fInput[curroffset] == 0) {
                                if (Error != null) Error(this, TokenizerErrorKind.EOFInString, "");
                                fLen = curroffset - fPos;
                            }
                        }
                        break;
                    case 39:
                        {
                            ++curroffset;
                            while ((fInput[curroffset] != 0) && (fInput[curroffset] != 39)) {

                                if (fInput[curroffset] == '\\') {
                                    ++curroffset;
                                    switch ((int)fInput[curroffset]) {
                                        case 'x':
                                        case 'X':
                                            if ((fInput[curroffset + 1] == 0) || (fInput[curroffset + 2] == 0))
                                            {
                                                if (Error != null) Error(this, TokenizerErrorKind.EOFInString, "");
                                                fLen = curroffset - fPos;
                                                return false;
                                            }
                                            else
                                                if (!Utilities.InSets3(fInput[curroffset + 1], '0', '9', 'A', 'F', 'a', 'f') ||
                                                    !Utilities.InSets3(fInput[curroffset + 2], '0', '9', 'A', 'F', 'a', 'f'))
                                            {
                                                if (Error != null) Error(this, TokenizerErrorKind.InvalidEscapeSequence, "");
                                                fLen = curroffset - fPos;
                                                return false;
                                            }
                                            else curroffset += 2;
                                            break;
                                        case 'u':
                                        case 'U':
                                            if ((fInput[curroffset + 1] == 0) ||
                                                (fInput[curroffset + 2] == 0) ||
                                                (fInput[curroffset + 3] == 0) ||
                                                (fInput[curroffset + 4] == 0)) {
                                                if (Error != null) Error(this, TokenizerErrorKind.EOFInString, "");
                                                fLen = curroffset - fPos;
                                                return false;
                                            }
                                            else
                                            if (!Utilities.InSets3(fInput[curroffset + 1], '0', '9', 'A', 'F', 'a', 'f') ||
                                               !Utilities.InSets3(fInput[curroffset + 2], '0', '9', 'A', 'F', 'a', 'f') ||
                                               !Utilities.InSets3(fInput[curroffset + 3], '0', '9', 'A', 'F', 'a', 'f') ||
                                               !Utilities.InSets3(fInput[curroffset + 4], '0', '9', 'A', 'F', 'a', 'f')) {
                                                if (Error != null) Error(this, TokenizerErrorKind.InvalidEscapeSequence, "");
                                                fLen = curroffset - fPos;
                                                return false;
                                            }
                                            else
                                                curroffset += 4;
                                            break;
                                        case 13:
                                            if (fInput[curroffset + 1] == 10) ++curroffset;
                                            break;
                                        case 10:
                                        case 'b':
                                        case 't':
                                        case 'n':
                                        case 'r':
                                        case 'v':
                                        case 'f':
                                        case '"':
                                        case 9:
                                        case '0':
                                        case '\\':
                                        case '\'':
                                            break;
                                        default:
                                            {
                                                if (Error != null) Error(this, TokenizerErrorKind.InvalidEscapeSequence, "");
                                                fLen = curroffset - fPos;
                                                return false;
                                            }
                                    } // case
                                }
                                ++curroffset;
                            }
                            fLen = curroffset - fPos + 1;
                            fToken = TokenKind.SingleQuoteString;
                            fTokenStr = new Char[fLen];
                            Array.Copy(fInput, fPos, fTokenStr, 0, fLen);
                        }
                        break;
                    default: {
                            fLen = 0;
                            if (Error != null) Error(this, TokenizerErrorKind.UnknownCharacter, fInput[curroffset].ToString());
                            fToken = TokenKind.Error;
                            return false;
                        }
                }
            }
            return true;
        }
    }
}