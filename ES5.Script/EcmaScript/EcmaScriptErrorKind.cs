using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public enum EcmaScriptErrorKind
    {
        FatalErrorWhileCompiling,
        OpeningParenthesisExpected,
        ClosingParenthesisExpected,
        OpeningBraceExpected,
        IdentifierExpected,
        ClosingBraceExpected,
        WhileExpected,
        SemicolonExpected,
        ColonExpected,
        CatchOrFinallyExpected,
        ClosingBracketExpected,
        SyntaxError,
        CommentError,
        EOFInRegex,
        EOFInString,
        EnterInRegex,
        InvalidEscapeSequence,
        UnknownCharacter,
        OnlyOneVariableAllowed,

        EInternalError = 10001,
        WithNotAllowedInStrict,
        CannotBreakHere,
        DuplicateIdentifier,
        CannotContinueHere,
        CannotReturnHere,
        OnlyOneDefaultAllowed,
        CannotAssignValueToExpression,
        UnknownLabelTarget,
        DuplicateLabel
    }
}