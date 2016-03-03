using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    /// <summary>Error type</summary>
    public enum TokenizerErrorKind
    {
        /// <summary>unknown character</summary>
        UnknownCharacter,
        /// <summary>comment not closed</summary>
        CommentError,
        InvalidEscapeSequence,
        /// <summary>invalid String</summary>
        EOFInString,
        EOFInRegex,
        EnterInRegex
    }
}