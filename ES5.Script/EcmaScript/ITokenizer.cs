using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public interface ITokenizer
    {
        void SetData(string input, string filename);
        void Next();
        void NextAsRegularExpression(out string aString, out string aModifiers); // sets the position after the regex

        bool LastWasEnter { get; }
        int Pos { get; }
        int Row { get; }
        int Col { get; }
        TokenKind Token { get; }
        string TokenStr { get; }
        Position Position { get; }
        Position EndPosition { get; }
        Position LastEndPosition { get; }
        PositionPair PositionPair { get; }

        object SaveState();
        void RestoreState(object o);

        event TokenizerError Error;
    }
}