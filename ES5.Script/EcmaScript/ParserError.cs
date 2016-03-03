using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ES5.Script.Properties;

namespace ES5.Script.EcmaScript
{
    public class ParserError : ParserMessage
    {
        readonly string fMessage;
        readonly EcmaScriptErrorKind fError;

        public override int Code
        {
            get { return (int)fError; }
        }

        public EcmaScriptErrorKind Error
        {
            get
            {
                return fError;
            }
        }

        public string Message
        {
            get
            {
                if (String.IsNullOrEmpty(fMessage) )
                  return base.ToString();

                return fMessage +" " + base.ToString();
            }
        }

        public override bool IsError
        {
            get
            {
                return true;
            }
        }


        public ParserError(Position position, EcmaScriptErrorKind error, string message):
            base(position)
        {
            fError = error;
            fMessage = message;
        }

        public override string ToString()
        {
            return Message;
        }


        public override string IntToString()
        {
            switch (fError) {
                case EcmaScriptErrorKind.OpeningParenthesisExpected: return Resources.eOpeningParenthesisExpected;
                case EcmaScriptErrorKind.OpeningBraceExpected: return Resources.eOpeningBraceExpected;
                case EcmaScriptErrorKind.ClosingParenthesisExpected: return Resources.eClosingParenthesisExpected;
                case EcmaScriptErrorKind.IdentifierExpected: return Resources.eIdentifierExpected;
                case EcmaScriptErrorKind.ClosingBraceExpected: return Resources.eClosingBraceExpected;
                case EcmaScriptErrorKind.WhileExpected: return Resources.eWhileExpected;
                case EcmaScriptErrorKind.SemicolonExpected: return Resources.eSemicolonExpected;
                case EcmaScriptErrorKind.ColonExpected: return Resources.eColonExpected;
                case EcmaScriptErrorKind.CatchOrFinallyExpected: return Resources.eCatchOrFinallyExpected;
                case EcmaScriptErrorKind.ClosingBracketExpected: return Resources.eClosingBracketExpected;
                case EcmaScriptErrorKind.SyntaxError: return Resources.eSyntaxError;
                case EcmaScriptErrorKind.CommentError: return Resources.eCommentError;
                case EcmaScriptErrorKind.EOFInRegex: return Resources.eEOFInRegex;
                case EcmaScriptErrorKind.EOFInString: return Resources.eEOFInString;
                case EcmaScriptErrorKind.InvalidEscapeSequence: return Resources.eInvalidEscapeSequence;
                case EcmaScriptErrorKind.UnknownCharacter: return Resources.eUnknownCharacter;
                case EcmaScriptErrorKind.OnlyOneVariableAllowed: return Resources.eOnlyOneVariableAllowed;
            } // case

            return "Unknown error";
        }
    }
}
