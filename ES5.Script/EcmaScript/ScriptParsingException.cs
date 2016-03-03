using ES5.Script.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class ScriptParsingException : ScriptException
    {
        PositionPair position;
        EcmaScriptErrorKind error;
        string msg;

        public ScriptParsingException(string aFilename, PositionPair aPosition, EcmaScriptErrorKind anError, string aMsg = "") :
            base(string.Format("{0}({1}:{2}) E{3} {4}",
                aFilename,
                aPosition.StartRow,
                aPosition.StartCol,
                (int)anError,
                ErrorToString(anError, aMsg)))
        {
            this.position = aPosition;
            this.error = anError;
            this.msg = aMsg;
        }

        public PositionPair Position { get { return this.position; } }
        public EcmaScriptErrorKind Error { get { return this.error; } }
        public string Msg { get { return this.msg; } }


        public static string ErrorToString(EcmaScriptErrorKind anError, string aMsg)
        {
            string result;
            switch (anError) {
                case EcmaScriptErrorKind.OpeningParenthesisExpected: result = Resources.eOpeningParenthesisExpected; break;
                case EcmaScriptErrorKind.OpeningBraceExpected: result = Resources.eOpeningBraceExpected; break;
                case EcmaScriptErrorKind.ClosingParenthesisExpected: result = Resources.eClosingParenthesisExpected; break;
                case EcmaScriptErrorKind.IdentifierExpected: result = Resources.eIdentifierExpected; break;
                case EcmaScriptErrorKind.ClosingBraceExpected: result = Resources.eClosingBraceExpected; break;
                case EcmaScriptErrorKind.WhileExpected: result = Resources.eWhileExpected; break;
                case EcmaScriptErrorKind.SemicolonExpected: result = Resources.eSemicolonExpected; break;
                case EcmaScriptErrorKind.ColonExpected: result = Resources.eColonExpected; break;
                case EcmaScriptErrorKind.CatchOrFinallyExpected: result = Resources.eCatchOrFinallyExpected; break;
                case EcmaScriptErrorKind.ClosingBracketExpected: result = Resources.eClosingBracketExpected; break;
                case EcmaScriptErrorKind.SyntaxError: result = Resources.eSyntaxError; break;
                case EcmaScriptErrorKind.CommentError: result = Resources.eCommentError; break;
                case EcmaScriptErrorKind.EOFInRegex: result = Resources.eEOFInRegex; break;
                case EcmaScriptErrorKind.EOFInString: result = Resources.eEOFInString; break;
                case EcmaScriptErrorKind.InvalidEscapeSequence: result = Resources.eInvalidEscapeSequence; break;
                case EcmaScriptErrorKind.UnknownCharacter: result = Resources.eUnknownCharacter; break;
                case EcmaScriptErrorKind.OnlyOneVariableAllowed: result = Resources.eOnlyOneVariableAllowed; break;
                case EcmaScriptErrorKind.WithNotAllowedInStrict: result = Resources.eWithNotAllowedInStrict; break;
                case EcmaScriptErrorKind.CannotBreakHere: result = String.Format(Resources.eCannotBreakHere, aMsg); break;
                case EcmaScriptErrorKind.CannotContinueHere: result = String.Format(Resources.eCannotBreakHere, aMsg); break;
                case EcmaScriptErrorKind.DuplicateIdentifier: result = String.Format(Resources.eDuplicateIdentifier, aMsg); break;
                case EcmaScriptErrorKind.EInternalError: result = String.Format(Resources.eInternalError, aMsg); break;
                case EcmaScriptErrorKind.CannotReturnHere: result = String.Format(Resources.eCannotReturnHere, aMsg); break;
                case EcmaScriptErrorKind.OnlyOneDefaultAllowed: result = String.Format(Resources.eOnlyOneDefaultAllowed, aMsg); break;
                case EcmaScriptErrorKind.CannotAssignValueToExpression: result = String.Format(Resources.eCannotAssignValueToExpression, aMsg); break;
                case EcmaScriptErrorKind.UnknownLabelTarget: result = String.Format(Resources.eUnknownLabelTarget, aMsg); break;
                case EcmaScriptErrorKind.DuplicateLabel: result = String.Format(Resources.eDuplicateIdentifier, aMsg); break;
                default:
                    result = Resources.eSyntaxError;
                    break;
            }
            return result;
        }
    }
}