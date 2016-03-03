using ES5.Script.EcmaScript.Internal;
using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class Parser
    {
        [Flags]
        internal enum ParseStatementFlags { None = 0, AllowFunction = 1, AllowGetSet = 2 };

        ITokenizer fTok;
        List<ParserMessage> fMessages;
        ReadOnlyCollection<ParserMessage> fMessagesWrapper;

        bool LookAheadGetSetWasName()
        {
            // current token is SET/GEt
            var lSave = fTok.SaveState();
            fTok.Next();

            var result = fTok.Token == TokenKind.Colon;

            fTok.RestoreState(lSave);

            return result;
        }

        internal void fTok_Error(Tokenizer caller, TokenizerErrorKind kind, string parameter)
        {
            fTok = caller;
            switch (kind)
            {
                case TokenizerErrorKind.CommentError: Error(EcmaScriptErrorKind.CommentError, ""); break;
                case TokenizerErrorKind.EOFInRegex: Error(EcmaScriptErrorKind.EOFInRegex, ""); break;
                case TokenizerErrorKind.EOFInString: Error(EcmaScriptErrorKind.EOFInString, ""); break;
                case TokenizerErrorKind.InvalidEscapeSequence: Error(EcmaScriptErrorKind.InvalidEscapeSequence, ""); break;
                case TokenizerErrorKind.UnknownCharacter: Error(EcmaScriptErrorKind.UnknownCharacter, ""); break;
                case TokenizerErrorKind.EnterInRegex: Error(EcmaScriptErrorKind.EnterInRegex, ""); break;
                default: Error((EcmaScriptErrorKind)Int32.MaxValue, ""); break;
            } // case
        }

        internal void Error(EcmaScriptErrorKind aCode, string aParameter)
        {
            fMessages.Add(new ParserError(fTok.Position, aCode, aParameter));
        }

        internal SourceElement ParseStatement(ParseStatementFlags aFlags)
        {
            SourceElement result = null;
            if (((ParseStatementFlags.AllowFunction & aFlags) != 0 && (fTok.Token == TokenKind.K_function)) ||
                ((ParseStatementFlags.AllowGetSet & aFlags) != 0 && (fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get)))
            {
                var lPos = fTok.Position;
                var lMode = FunctionDeclarationType.None;
                if (fTok.Token == TokenKind.K_set)
                {
                    lMode = FunctionDeclarationType.Set;
                    //fTok.Next();
                }
                else
                if (fTok.Token == TokenKind.K_get)
                {
                    lMode = FunctionDeclarationType.Get;
                    //fTok.Next();
                }

                fTok.Next();
                var lName = (String)null;
                if (fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get || fTok.Token == TokenKind.Identifier)
                {
                    lName = fTok.TokenStr;
                    fTok.Next();
                }
                if (fTok.Token != TokenKind.OpeningParenthesis)
                {
                    Error(EcmaScriptErrorKind.OpeningParenthesisExpected, "");
                    return null;
                }
                fTok.Next();
                var lParams = new List<ParameterDeclaration>();
                if (fTok.Token == TokenKind.ClosingParenthesis)
                    fTok.Next();
                else
                {
                    /*loop*/
                    while (true)
                    {
                        if (!(fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get || fTok.Token == TokenKind.Identifier))
                        {
                            Error(EcmaScriptErrorKind.IdentifierExpected, "");
                            return null;
                        }

                        lParams.Add(new ParameterDeclaration(fTok.PositionPair, fTok.TokenStr));
                        fTok.Next();

                        if (fTok.Token == TokenKind.Comma)
                        {
                            fTok.Next();
                        }
                        else if (fTok.Token == TokenKind.ClosingParenthesis)
                        {
                            fTok.Next();
                            break;
                        }
                        else
                        {
                            Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                            return null;
                        }
                    }
                }

                if (fTok.Token != TokenKind.CurlyOpen)
                {
                    Error(EcmaScriptErrorKind.OpeningBraceExpected, "");
                    return null;
                }

                var lItems = new List<SourceElement>();

                fTok.Next();
                while (fTok.Token != TokenKind.CurlyClose)
                {
                    if (fTok.Token == TokenKind.EOF)
                    {
                        Error(EcmaScriptErrorKind.ClosingBraceExpected, "");
                        return null;
                    }
                    var lState = ParseStatement(ParseStatementFlags.AllowFunction);
                    if (lState == null) return null;
                    lItems.Add(lState);
                }
                fTok.Next();
                var lPositionPair = new PositionPair(lPos, fTok.LastEndPosition);
                return new FunctionDeclarationElement(lPositionPair, lMode, lName, lParams, lItems);
            }
            else
            {
                if (fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get || fTok.Token == TokenKind.Identifier)
                {
                    var lObj = fTok.SaveState();
                    var lPos = fTok.Position;
                    var lIdent = fTok.TokenStr;
                    fTok.Next();
                    if (fTok.Token == TokenKind.Colon)
                    {
                        fTok.Next();
                        var lSt = ParseStatement(ParseStatementFlags.None);
                        if (lSt == null) return null;
                        return new LabelledStatement(new PositionPair(lPos, fTok.LastEndPosition), lIdent, (Statement)lSt);
                    }
                    else
                        fTok.RestoreState(lObj);
                }

                switch (fTok.Token)
                {
                    case TokenKind.CurlyOpen: return ParseBlockStatement();
                    case TokenKind.K_var: return ParseVarStatement(true, true);
                    case TokenKind.Semicolon: return ParseEmptyStatement();
                    case TokenKind.K_if: return ParseIfStatement();
                    case TokenKind.K_do: return ParseDoStatement();
                    case TokenKind.K_while: return ParseWhileStatement();
                    case TokenKind.K_for: return ParseForStatement();
                    case TokenKind.K_continue: return ParseContinueStatement();
                    case TokenKind.K_break: return ParseBreakStatement();
                    case TokenKind.K_return: return ParseReturnStatement();
                    case TokenKind.K_with: return ParseWithStatement();
                    case TokenKind.K_switch: return ParseSwitchStatement();
                    case TokenKind.K_throw: return ParseThrowStatement();
                    case TokenKind.K_try: return ParseTryStatement();
                    case TokenKind.K_debugger: return ParseDebuggerStatement();
                    default:
                        {
                            var lPos = fTok.Position;
                            var lExpr = ParseExpression(true);
                            if (lExpr == null) return null;
                            if (fTok.Token == TokenKind.Comma)
                            {
                                var lItems = new List<SourceElement>();
                                lItems.Add(new ExpressionStatement(new PositionPair(lPos, fTok.LastEndPosition), lExpr));
                                while (fTok.Token == TokenKind.Comma)
                                {
                                    fTok.Next();
                                    var lSp = fTok.Position;
                                    lExpr = ParseExpression(true);
                                    if (lExpr == null) return null;
                                    lItems.Add(new ExpressionStatement(new PositionPair(lSp, fTok.LastEndPosition), lExpr));
                                }
                                if (fTok.Token == TokenKind.Semicolon) fTok.Next();
                                result = new BlockStatement(new PositionPair(lPos, fTok.LastEndPosition), lItems);
                            }
                            else
                            {
                                if (fTok.Token == TokenKind.Semicolon) fTok.Next();
                                result = new ExpressionStatement(new PositionPair(lPos, fTok.LastEndPosition), lExpr);
                            }
                        }
                        break;
                } // case

                return result;
            }
        }

        internal BlockStatement ParseBlockStatement()
        {
            var lPos = fTok.Position;
            var lItems = new List<SourceElement>();
            fTok.Next();
            while (fTok.Token != TokenKind.CurlyClose)
            {
                if (fTok.Token == TokenKind.EOF)
                {
                    Error(EcmaScriptErrorKind.ClosingBraceExpected, "");
                    return null;
                }

                var lState = ParseStatement(ParseStatementFlags.AllowFunction);
                if (lState == null)
                    break;

                lItems.Add(lState);
            }
            fTok.Next();
            return new BlockStatement(new PositionPair(lPos, fTok.LastEndPosition), lItems);
        }

        internal VariableStatement ParseVarStatement(bool aAllowIn, bool aParseSemicolon)
        {
            var lPos = fTok.Position;
            var lItems = new List<VariableDeclaration>();
            fTok.Next();
            while (true)
            {
                var lPosVar = fTok.Position;
                if (!(fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get || fTok.Token == TokenKind.Identifier))
                {
                    Error(EcmaScriptErrorKind.IdentifierExpected, "");
                    return null;
                }
                var lName = fTok.TokenStr;
                fTok.Next();
                ExpressionElement lValue;
                if (fTok.Token == TokenKind.Assign)
                {
                    fTok.Next();
                    lValue = ParseExpression(aAllowIn);
                    if (lValue == null) return null;
                }
                else
                    lValue = null;

                lItems.Add(new VariableDeclaration(new PositionPair(lPosVar, fTok.LastEndPosition), lName, lValue));
                if (fTok.Token == TokenKind.Comma)
                {
                    fTok.Next();
                }
                else if (fTok.Token == TokenKind.Semicolon)
                {
                    if (aParseSemicolon)
                        fTok.Next();
                    break;
                }
                else
                {
                    break; // implicit semi
                }
            }
            return new VariableStatement(new PositionPair(lPos, fTok.LastEndPosition), lItems);
        }

        internal EmptyStatement ParseEmptyStatement()
        {
            var result = fTok.PositionPair;
            fTok.Next();
            return new EmptyStatement(result);
        }

        internal IfStatement ParseIfStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            if (fTok.Token != TokenKind.OpeningParenthesis)
            {
                Error(EcmaScriptErrorKind.OpeningParenthesisExpected, "");
                return null;
            }
            fTok.Next();
            var lExpr = ParseCommaExpression(true);
            if (lExpr == null) return null;
            if (fTok.Token != TokenKind.ClosingParenthesis)
            {
                Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                return null;
            }
            fTok.Next();
            var lTrue = (Statement)ParseStatement(ParseStatementFlags.None);
            if (lTrue == null) return null;
            var lFalse = (Statement)null;
            if (fTok.Token == TokenKind.K_else)
            {
                fTok.Next();
                lFalse = (Statement)ParseStatement(ParseStatementFlags.None);
                if (lFalse == null) return null;
            }
            return new IfStatement(new PositionPair(lPos, fTok.LastEndPosition), lExpr, lTrue, lFalse);
        }

        internal DoStatement ParseDoStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();

            var lStatement = (Statement)ParseStatement(ParseStatementFlags.None);

            if (fTok.Token != TokenKind.K_while)
            {
                Error(EcmaScriptErrorKind.WhileExpected, "");
                return null;
            }

            fTok.Next();

            if (fTok.Token != TokenKind.OpeningParenthesis)
            {
                Error(EcmaScriptErrorKind.OpeningParenthesisExpected, "");
                return null;
            }
            fTok.Next();
            var lExpr = ParseCommaExpression(true);
            if (lExpr == null) return null;
            if (fTok.Token != TokenKind.ClosingParenthesis)
            {
                Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                return null;
            }
            fTok.Next();
            if (fTok.Token == TokenKind.Semicolon) fTok.Next();
            return new DoStatement(new PositionPair(lPos, fTok.LastEndPosition), lStatement, lExpr);
        }

        internal WhileStatement ParseWhileStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            if (fTok.Token != TokenKind.OpeningParenthesis)
            {
                Error(EcmaScriptErrorKind.OpeningParenthesisExpected, "");
                return null;
            }
            fTok.Next();
            var lExpr = ParseCommaExpression(true);
            if (lExpr == null) return null;
            if (fTok.Token != TokenKind.ClosingParenthesis)
            {
                Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                return null;
            }
            fTok.Next();


            var lStatement = (Statement)ParseStatement(ParseStatementFlags.None);

            if (fTok.Token == TokenKind.Semicolon) fTok.Next();
            return new WhileStatement(new PositionPair(lPos, fTok.LastEndPosition), lExpr, lStatement);
        }

        internal Statement ParseForStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            if (fTok.Token != TokenKind.OpeningParenthesis) {
                Error(EcmaScriptErrorKind.OpeningParenthesisExpected, "");
                return null;
            }
            fTok.Next();
            var lVars = (VariableStatement)null;
            var lInit = (ExpressionElement)null;
            if (fTok.Token == TokenKind.K_var) {
                lVars = ParseVarStatement(false, false);
                if (lVars == null) return null;
                //if lVars.Items.Count != 1 then begin
                //      Error(EcmaScriptErrorKind.OnlyOneVariableAllowed, "");
                //  end;
            } else if (fTok.Token != TokenKind.Semicolon) {
                lInit = ParseCommaExpression(false);
                if (lInit == null) return null;
            }

            if (fTok.Token == TokenKind.K_in) {
                fTok.Next();
                var lIn = ParseCommaExpression(true);
                if (fTok.Token != TokenKind.ClosingParenthesis) {
                    Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                    return null;
                }
                fTok.Next();
                var lSt = (Statement)ParseStatement(ParseStatementFlags.None);
                if (lSt == null) return null;
                if (lVars != null)
                    return new ForInStatement(new PositionPair(lPos, fTok.LastEndPosition), lVars.Items[0], lIn, lSt);
                else
                    return new ForInStatement(new PositionPair(lPos, fTok.LastEndPosition), lInit, lIn, lSt);
            } else {
                if (fTok.Token != TokenKind.Semicolon) {
                    Error(EcmaScriptErrorKind.SemicolonExpected, "");
                    return null;
                }
                fTok.Next();
                var lCondition = (ExpressionElement)null;
                if (fTok.Token != TokenKind.Semicolon) {
                    lCondition = ParseCommaExpression(true);
                    if (lCondition == null) return null;
                }

                if (fTok.Token != TokenKind.Semicolon) {
                    Error(EcmaScriptErrorKind.SemicolonExpected, "");
                    return null;
                }
                fTok.Next();

                var lIncrement = (ExpressionElement)null;
                if (fTok.Token != TokenKind.ClosingParenthesis) {
                    lIncrement = ParseCommaExpression(true);
                    if (lIncrement == null) return null;
                }

                if (fTok.Token != TokenKind.ClosingParenthesis) {
                    Error(EcmaScriptErrorKind.ClosingBraceExpected, "");
                    return null;
                }
                fTok.Next();
                var lSt = (Statement)ParseStatement(ParseStatementFlags.None);
                if (lSt == null) return null;
                if (lVars != null)
                    return new ForStatement(new PositionPair(lPos, fTok.LastEndPosition), lVars.Items, lCondition, lIncrement, lSt);
                else
                    return new ForStatement(new PositionPair(lPos, fTok.LastEndPosition), lInit, lCondition, lIncrement, lSt);
            }
        }


        internal ContinueStatement ParseContinueStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            var lIdent = (String)null;
            if (!fTok.LastWasEnter && (fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get || fTok.Token == TokenKind.Identifier))
            {
                lIdent = fTok.TokenStr;
                fTok.Next();
            }

            if (fTok.Token == TokenKind.Semicolon)
                fTok.Next();

            return new ContinueStatement(new PositionPair(lPos, fTok.LastEndPosition), lIdent);
        }

        internal BreakStatement ParseBreakStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            var lIdent = (String)null;
            if (!fTok.LastWasEnter && (fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get || fTok.Token == TokenKind.Identifier))
            {
                lIdent = fTok.TokenStr;
                fTok.Next();
            }

            if (fTok.Token == TokenKind.Semicolon) fTok.Next();
            return new BreakStatement(new PositionPair(lPos, fTok.LastEndPosition), lIdent);
        }

        internal ReturnStatement ParseReturnStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            var lExpr = (ExpressionElement)null;
            if (!fTok.LastWasEnter && !(fTok.Token == TokenKind.Semicolon || fTok.Token == TokenKind.K_else || fTok.Token == TokenKind.K_default || fTok.Token == TokenKind.CurlyClose))
            {
                lExpr = ParseCommaExpression(true);
                if (lExpr == null) return null;
            }

            if (fTok.Token == TokenKind.Semicolon) fTok.Next();
            return new ReturnStatement(new PositionPair(lPos, fTok.LastEndPosition), lExpr);
        }

        internal WithStatement ParseWithStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            if (fTok.Token != TokenKind.OpeningParenthesis) {
                Error(EcmaScriptErrorKind.OpeningParenthesisExpected, "");
                return null;
            }
            fTok.Next();
            var lExpr = ParseCommaExpression(true);
            if (lExpr == null) return null;
            if (fTok.Token != TokenKind.ClosingParenthesis) {
                Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                return null;
            }
            fTok.Next();


            var lStatement = (Statement)ParseStatement(ParseStatementFlags.None);

            if (fTok.Token == TokenKind.Semicolon) fTok.Next();
            return new WithStatement(new PositionPair(lPos, fTok.LastEndPosition), lExpr, lStatement);
        }

        internal SwitchStatement ParseSwitchStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            if (fTok.Token != TokenKind.OpeningParenthesis) {
                Error(EcmaScriptErrorKind.OpeningParenthesisExpected, "");
                return null;
            }
            fTok.Next();
            var lExpr = ParseCommaExpression(true);
            if (lExpr == null) return null;
            if (fTok.Token != TokenKind.ClosingParenthesis) {
                Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                return null;
            }
            fTok.Next();

            if (fTok.Token != TokenKind.CurlyOpen) {
                Error(EcmaScriptErrorKind.OpeningBraceExpected, "");
                return null;
            }
            fTok.Next();
            var lClauses = new List<CaseClause>();
            while (true) {
                if (fTok.Token == TokenKind.CurlyClose)
                    break;
                var lCaseExpr = (ExpressionElement)null;
                var lItemPos = fTok.Position;
                if (fTok.Token == TokenKind.K_case) {
                    fTok.Next();
                    lCaseExpr = ParseCommaExpression(true);
                    if (lCaseExpr == null) return null;
                } else if (fTok.Token == TokenKind.K_default) {
                    fTok.Next();
                } else {
                    Error(EcmaScriptErrorKind.ClosingBraceExpected, "");
                    return null;
                }

                if (fTok.Token != TokenKind.Colon) {
                    Error(EcmaScriptErrorKind.ColonExpected, "");
                    return null;
                }
                fTok.Next();
                var lStatements = new List<Statement>();
                while (!(fTok.Token == TokenKind.K_case || fTok.Token == TokenKind.K_default || fTok.Token == TokenKind.CurlyClose)) {
                    var lSt = (Statement)ParseStatement(ParseStatementFlags.None);
                    if (lSt == null) return null;
                    lStatements.Add(lSt);
                }
                lClauses.Add(new CaseClause(new PositionPair(lItemPos, fTok.LastEndPosition), lCaseExpr, lStatements));
            }
            fTok.Next();
            return new SwitchStatement(new PositionPair(lPos, fTok.LastEndPosition), lExpr, lClauses);
        }

        internal ThrowStatement ParseThrowStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            var lExpr = (ExpressionElement)null;
            lExpr = ParseCommaExpression(true);
            if (lExpr == null) return null;


            if (fTok.Token == TokenKind.Semicolon) fTok.Next();
            return new ThrowStatement(new PositionPair(lPos, fTok.LastEndPosition), lExpr);
        }

        internal TryStatement ParseTryStatement()
        {
            var lPos = fTok.Position;

            fTok.Next();
            if (fTok.Token != TokenKind.CurlyOpen) {
                Error(EcmaScriptErrorKind.OpeningBraceExpected, "");
                return null;
            }

            var lBody = ParseBlockStatement();
            if (lBody == null) return null;

            var lCatch = (CatchBlock)null;
            var lFinally = (BlockStatement)null;
            if (fTok.Token == TokenKind.K_catch) {
                var lCp = fTok.Position;
                fTok.Next();
                if (fTok.Token != TokenKind.OpeningParenthesis) {
                    Error(EcmaScriptErrorKind.OpeningParenthesisExpected, "");
                    return null;
                }
                fTok.Next();
                if (!(fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get || fTok.Token == TokenKind.Identifier)) {
                    Error(EcmaScriptErrorKind.IdentifierExpected, "");
                    return null;
                }

                var lIdent = fTok.TokenStr;
                fTok.Next();
                if (fTok.Token != TokenKind.ClosingParenthesis) {
                    Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                    return null;
                }
                fTok.Next();
                if (fTok.Token != TokenKind.CurlyOpen) {
                    Error(EcmaScriptErrorKind.OpeningBraceExpected, "");
                    return null;
                }

                var lCatchBody = ParseBlockStatement();
                if (lCatchBody == null) return null;
                lCatch = new CatchBlock(new PositionPair(lCp, fTok.LastEndPosition), lIdent, lCatchBody);
            }

            if (fTok.Token == TokenKind.K_finally) {
                var lCp = fTok.Position;
                fTok.Next();
                if (fTok.Token != TokenKind.CurlyOpen) {
                    Error(EcmaScriptErrorKind.OpeningBraceExpected, "");
                    return null;
                }

                lFinally = ParseBlockStatement();
                if (lFinally == null) return null;
            }

            if ((lCatch == null) && (lFinally == null)) {
                Error(EcmaScriptErrorKind.CatchOrFinallyExpected, "");
                return null;
            }
            return new TryStatement(new PositionPair(lPos, fTok.LastEndPosition), lBody, lCatch, lFinally);
        }

        internal DebuggerStatement ParseDebuggerStatement()
        {
            var lPos = fTok.Position;
            fTok.Next();
            if (fTok.Token == TokenKind.Semicolon) fTok.Next();
            return new DebuggerStatement(new PositionPair(lPos, fTok.LastEndPosition));
        }

        internal ExpressionElement ParseExpression(bool aAllowIn)
        {
            var lLeft = ParseConditionalExpression(aAllowIn);
            if (lLeft == null) return null;
            while (Utilities.InSet(fTok.Token,
                  TokenKind.Assign,
                  TokenKind.PlusAssign, // +=
                  TokenKind.MinusAssign,// -=
                  TokenKind.MultiplyAssign, // *=
                  TokenKind.ModulusAssign, // %=
                  TokenKind.ShiftLeftAssign, // <<=
                  TokenKind.ShiftRightSignedAssign,// >>=
                  TokenKind.ShiftRightUnsignedAssign, // >>>=
                  TokenKind.AndAssign, // &=
                  TokenKind.OrAssign, // |=
                  TokenKind.XorAssign, // ^=
                  TokenKind.DivideAssign)) { // /=
                BinaryOperator lOp;
                switch (fTok.Token) {
                    case TokenKind.Assign: lOp = BinaryOperator.Assign; break;
                    case TokenKind.PlusAssign: lOp = BinaryOperator.PlusAssign; break;
                    case TokenKind.MinusAssign: lOp = BinaryOperator.MinusAssign; break;
                    case TokenKind.MultiplyAssign: lOp = BinaryOperator.MultiplyAssign; break;
                    case TokenKind.ModulusAssign: lOp = BinaryOperator.ModulusAssign; break;
                    case TokenKind.ShiftLeftAssign: lOp = BinaryOperator.ShiftLeftAssign; break;
                    case TokenKind.ShiftRightSignedAssign: lOp = BinaryOperator.ShiftRightSignedAssign; break;
                    case TokenKind.ShiftRightUnsignedAssign: lOp = BinaryOperator.ShiftRightUnsignedAssign; break;
                    case TokenKind.AndAssign: lOp = BinaryOperator.AndAssign; break;
                    case TokenKind.OrAssign: lOp = BinaryOperator.OrAssign; break;
                    case TokenKind.XorAssign: lOp = BinaryOperator.XorAssign; break;
                    default: lOp = BinaryOperator.DivideAssign; break; // TokenKind.DivideAssign
                }
                fTok.Next();
                var lRight = ParseExpression(aAllowIn);
                if (lRight == null) return null;
                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, lOp);
            }
            return lLeft;
        }

        internal ExpressionElement ParseCommaExpression(bool aAllowIn)
        {
            var lPos = fTok.Position;
            var result = ParseExpression(aAllowIn);
            if (fTok.Token == TokenKind.Comma)
            {
                var lItems = new List<ExpressionElement>();
                lItems.Add(result);
                while (fTok.Token == TokenKind.Comma)
                {
                    fTok.Next();
                    lItems.Add(ParseExpression(aAllowIn));
                }

                return new CommaSeparatedExpression(new PositionPair(lPos, fTok.EndPosition), lItems);
            }
            return result;
        }

        internal ExpressionElement ParseConditionalExpression(bool aAllowIn)
        {
            var lLeft = ParseLogicalOrExpression(aAllowIn);
            if (lLeft == null) return null;
            while (fTok.Token == TokenKind.ConditionalOperator) {
                fTok.Next();
                var lMiddle = ParseExpression(aAllowIn);
                if (lMiddle == null) return null;
                if (fTok.Token != TokenKind.Colon) {
                    Error(EcmaScriptErrorKind.ColonExpected, "");
                    return null;
                }
                fTok.Next();
                var lRight = ParseExpression(aAllowIn);
                if (lRight == null) return null;
                lLeft = new ConditionalExpression(lLeft.PositionPair, lLeft, lMiddle, lRight);
            }

            return lLeft;
        }

        internal ExpressionElement ParseLogicalOrExpression(bool aAllowIn)
        {
            var lLeft = ParseBitwiseOrExpression(aAllowIn);
            if (lLeft == null) return null;
            while (fTok.Token == TokenKind.DoubleAnd) {
                fTok.Next();
                var lRight = ParseBitwiseOrExpression(aAllowIn);
                if (lRight == null) return null;
                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, BinaryOperator.DoubleAnd);
            }

            while (fTok.Token == TokenKind.DoubleOr)
            {
                fTok.Next();
                var lRight = ParseBitwiseOrExpression(aAllowIn);
                if (lRight == null) return null;
                while (fTok.Token == TokenKind.DoubleAnd)
                {
                    fTok.Next();
                    var lRight2 = ParseBitwiseOrExpression(aAllowIn);
                    if (lRight2 == null) return null;
                    lRight = new BinaryExpression(lRight.PositionPair, lRight, lRight2, BinaryOperator.DoubleAnd);
                }

                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, BinaryOperator.DoubleOr);
            }
            return lLeft;
        }

        internal ExpressionElement ParseBitwiseOrExpression(bool aAllowIn)
        {
            var lLeft = ParseEqualityExpression(aAllowIn);
            if (lLeft == null) return null;

            while (fTok.Token == TokenKind.And) {
                fTok.Next();
                var lRight = ParseEqualityExpression(aAllowIn);
                if (lRight == null) return null;
                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, BinaryOperator.And);
            }

            while (fTok.Token == TokenKind.Xor) {
                fTok.Next();
                var lRight = ParseEqualityExpression(aAllowIn);
                if (lRight == null) return null;
                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, BinaryOperator.Xor);
            }

            while (fTok.Token == TokenKind.Or)
            {
                fTok.Next();
                var lRight = ParseEqualityExpression(aAllowIn);
                if (lRight == null) return null;
                while (fTok.Token == TokenKind.And)
                {
                    fTok.Next();
                    var lRight2 = ParseEqualityExpression(aAllowIn);
                    if (lRight2 == null) return null;
                    lRight = new BinaryExpression(lRight.PositionPair, lRight, lRight2, BinaryOperator.And);
                }

                while (fTok.Token == TokenKind.Xor)
                {
                    fTok.Next();
                    var lRight2 = ParseEqualityExpression(aAllowIn);
                    if (lRight2 == null) return null;
                    lRight = new BinaryExpression(lRight.PositionPair, lRight, lRight2, BinaryOperator.Xor);
                }

                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, BinaryOperator.Or);
            }
            return lLeft;
        }

        internal ExpressionElement ParseEqualityExpression(bool aAllowIn)
        {
            var lLeft = ParseRelationalExpression(aAllowIn);
            if (lLeft == null) return null;
            while (fTok.Token == TokenKind.Equal ||
                fTok.Token == TokenKind.NotEqual ||
              fTok.Token == TokenKind.StrictEqual ||
              fTok.Token == TokenKind.StrictNotEqual) { // /=
                BinaryOperator lOp;
                switch (fTok.Token) {
                    case TokenKind.Equal: lOp = BinaryOperator.Equal; break;
                    case TokenKind.NotEqual: lOp = BinaryOperator.NotEqual; break;
                    case TokenKind.StrictEqual: lOp = BinaryOperator.StrictEqual; break;
                    default: lOp = BinaryOperator.StrictNotEqual; break; //TokenKind.StrictNotEqual
                }
                fTok.Next();
                var lRight = ParseRelationalExpression(aAllowIn);
                if (lRight == null) return null;
                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, lOp);
            }
            return lLeft;
        }

        internal ExpressionElement ParseRelationalExpression(bool aAllowIn)
        {
            var lLeft = ParseShiftExpression();
            if (lLeft == null) return null;
            while ((fTok.Token == TokenKind.Less || fTok.Token == TokenKind.Greater ||
                    fTok.Token == TokenKind.LessOrEqual || fTok.Token == TokenKind.GreaterOrEqual ||
                    fTok.Token == TokenKind.K_instanceof) || (aAllowIn && fTok.Token == TokenKind.K_in)) {
                BinaryOperator lOp;
                switch (fTok.Token) {
                    case TokenKind.Less: lOp = BinaryOperator.Less; break;
                    case TokenKind.Greater: lOp = BinaryOperator.Greater; break;
                    case TokenKind.LessOrEqual: lOp = BinaryOperator.LessOrEqual; break;
                    case TokenKind.GreaterOrEqual: lOp = BinaryOperator.GreaterOrEqual; break;
                    case TokenKind.K_instanceof: lOp = BinaryOperator.InstanceOf; break;
                    default: lOp = BinaryOperator.In; break; //TokenKind.In
                }
                fTok.Next();
                var lRight = ParseShiftExpression();
                if (lRight == null) return null;
                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, lOp);
            }
            return lLeft;
        }

        internal ExpressionElement ParseShiftExpression()
        {
            var lLeft = ParseAdditiveExpression();
            if (lLeft == null) return null;
            while (fTok.Token == TokenKind.ShiftLeft ||
                fTok.Token == TokenKind.ShiftRightSigned ||
              fTok.Token == TokenKind.ShiftRightUnsigned) {
                BinaryOperator lOp;
                switch (fTok.Token) {
                    case TokenKind.ShiftLeft: lOp = BinaryOperator.ShiftLeft; break;
                    case TokenKind.ShiftRightSigned: lOp = BinaryOperator.ShiftRightSigned; break;
                    default: lOp = BinaryOperator.ShiftRightUnsigned; break; //TokenKind.ShiftRightUnsigned
                }
                fTok.Next();
                var lRight = ParseAdditiveExpression();
                if (lRight == null) return null;
                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, lOp);
            }
            return lLeft;
        }

        internal ExpressionElement ParseAdditiveExpression()
        {
            var lLeft = ParseMultiplicativeExpression();
            if (lLeft == null) return null;

            BinaryOperator lOp;
            while (fTok.Token == TokenKind.Plus || fTok.Token == TokenKind.Minus) {
                if (fTok.Token == TokenKind.Plus) lOp = BinaryOperator.Plus; else lOp = BinaryOperator.Minus;
                fTok.Next();
                var lRight = ParseMultiplicativeExpression();
                if (lRight == null) return null;

                lLeft = new BinaryExpression(lLeft.PositionPair, lLeft, lRight, lOp);
            }
            return lLeft;
        }

        internal ExpressionElement ParseMultiplicativeExpression()
        {
            var lLeft = ParseUnaryExpression();
            if (lLeft == null) return null;

            while (fTok.Token == TokenKind.Multiply || fTok.Token == TokenKind.Divide || fTok.Token == TokenKind.Modulus) {
                BinaryOperator lOp;
                switch (fTok.Token) {
                    case TokenKind.Multiply: lOp = BinaryOperator.Multiply; break;
                    case TokenKind.Divide: lOp = BinaryOperator.Divide; break;
                    default: lOp = BinaryOperator.Modulus; break;
                }
                fTok.Next();
                var lRight = ParseUnaryExpression();
                if (lRight == null) return null;

                return new BinaryExpression(lLeft.PositionPair, lLeft, lRight, lOp);
            }
            return lLeft;
        }

        internal ExpressionElement ParseUnaryExpression()
        {
            UnaryOperator lUnaryOp;
            switch (fTok.Token) {
                case TokenKind.K_delete: lUnaryOp = UnaryOperator.Delete; break;
                case TokenKind.K_void: lUnaryOp = UnaryOperator.Void; break;
                case TokenKind.K_typeof: lUnaryOp = UnaryOperator.TypeOf; break;
                case TokenKind.Increment: lUnaryOp = UnaryOperator.PreIncrement; break;
                case TokenKind.Decrement: lUnaryOp = UnaryOperator.PreDecrement; break;
                case TokenKind.Plus: lUnaryOp = UnaryOperator.Plus; break;
                case TokenKind.Minus: lUnaryOp = UnaryOperator.Minus; break;
                case TokenKind.Not: lUnaryOp = UnaryOperator.BoolNot; break;
                case TokenKind.BitwiseNot: lUnaryOp = UnaryOperator.BinaryNot; break;
                default: return ParsePostfixExpression();
            }

            var lPos = fTok.Position;
            fTok.Next();
            var lVal = ParseUnaryExpression();
            if (lVal == null) return null;

            return new UnaryExpression(new PositionPair(lPos, fTok.LastEndPosition), lVal, lUnaryOp);
        }

        internal ExpressionElement ParsePostfixExpression()
        {
            var lVal = ParseLeftHandSideExpression(true);
            if (lVal == null) return null;

            if (fTok.LastWasEnter) return lVal;

            if (fTok.Token == TokenKind.Decrement)
            {
                lVal = new UnaryExpression(new PositionPair(lVal.PositionPair.StartPos, lVal.PositionPair.StartRow, lVal.PositionPair.StartCol, fTok.LastEndPosition.Pos, fTok.LastEndPosition.Row, fTok.LastEndPosition.Col, lVal.PositionPair.File), lVal, UnaryOperator.PostDecrement);
                fTok.Next();
            }
            else if (fTok.Token == TokenKind.Increment)
            {
                lVal = new UnaryExpression(new PositionPair(lVal.PositionPair.StartPos, lVal.PositionPair.StartRow, lVal.PositionPair.StartCol, fTok.LastEndPosition.Pos, fTok.LastEndPosition.Row, fTok.LastEndPosition.Col, lVal.PositionPair.File), lVal, UnaryOperator.PostIncrement);
                fTok.Next();
            }
            return lVal;
        }

        internal ExpressionElement ParseLeftHandSideExpression(bool aParseParameters)
        {
            ExpressionElement lVal;

            switch (fTok.Token) {
                case TokenKind.K_function:
                    {
                        var lFunc = ParseStatement(ParseStatementFlags.AllowFunction);
                        if (lFunc == null) return null;
                        lVal = new FunctionExpression(lFunc.PositionPair, lFunc as FunctionDeclarationElement);
                    }
                    break;
                case TokenKind.K_this:
                    {
                        lVal = new ThisExpression(fTok.PositionPair);
                        fTok.Next();
                    }
                    break;
                case TokenKind.K_set:
                case TokenKind.K_get:
                case TokenKind.Identifier:
                    {
                        lVal = new IdentifierExpression(fTok.PositionPair, fTok.TokenStr);
                        fTok.Next();
                    }
                    break;
                case TokenKind.OpeningBracket:
                    {
                        var lPos = fTok.Position;
                        fTok.Next();
                        var lArgs = new List<ExpressionElement>();

                        ExpressionElement lSub;
                        if (fTok.Token != TokenKind.ClosingBracket) {
                            //if fTok.Token = TokenKind.Comma then begin
                            //fTok.Next;
                            //end;
                            while (true) {
                                if (fTok.Token == TokenKind.Comma || fTok.Token == TokenKind.ClosingBracket)
                                    lSub = null;
                                else {
                                    lSub = ParseExpression(true);
                                    if (lSub == null) return null;
                                }

                                lArgs.Add(lSub);
                                if (fTok.Token == TokenKind.Comma) {
                                    fTok.Next();
                                    //if fTok.Token = TokenKind.ClosingBracket then
                                    //lArgs.RemoveAt(lArgs.Count-1);
                                } else
                                if (fTok.Token == TokenKind.ClosingBracket) break;
                                else
                                {
                                    Error(EcmaScriptErrorKind.ClosingBracketExpected, "");
                                    return null;
                                }
                            }
                        }
                        fTok.Next();
                        lVal = new ArrayLiteralExpression(new PositionPair(lPos, fTok.LastEndPosition), lArgs);
                    }
                    break;
                case TokenKind.CurlyOpen:
                    {
                        var lPos = fTok.Position;
                        fTok.Next();
                        var lArgs = new List<PropertyAssignment>();
                        if (fTok.Token != TokenKind.CurlyClose)
                        {
                            while (true)
                            {
                                PropertyAssignment lSub;
                                if (fTok.Token == TokenKind.Comma || fTok.Token == TokenKind.CurlyClose)
                                    lSub = null;
                                else
                                {
                                    ExpressionElement lName;
                                    ExpressionElement lValue;
                                    var lMode = FunctionDeclarationType.None;
                                    if ((fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get) && !LookAheadGetSetWasName())
                                    {
                                        lName = null;
                                        lMode = fTok.Token == TokenKind.K_set ? FunctionDeclarationType.Set : FunctionDeclarationType.Get;

                                        var lTmp = ParseStatement(ParseStatementFlags.AllowGetSet) as FunctionDeclarationElement;
                                        if (lTmp == null) return null;
                                        lName = new StringExpression(lTmp.PositionPair, lTmp.Identifier);
                                        lValue = new FunctionExpression(lTmp.PositionPair, lTmp);
                                    }
                                    else
                                    {
                                        lName = ParseLeftHandSideExpression(false);
                                        if (lName == null) return null;
                                        if (!(lName is PropertyBaseExpression))
                                        {
                                            Error(EcmaScriptErrorKind.SyntaxError, "");
                                            return null;
                                        }
                                        if (fTok.Token != TokenKind.Colon)
                                        {
                                            Error(EcmaScriptErrorKind.ColonExpected, "");
                                            return null;
                                        }
                                        fTok.Next();
                                        lValue = ParseExpression(true);
                                        if (lValue == null) return null;
                                    }

                                    var lPositionPair = lName == null ? lValue.PositionPair :
                                        new PositionPair(lName.PositionPair.StartPos, lName.PositionPair.StartRow, lName.PositionPair.StartCol, fTok.LastEndPosition.Pos, fTok.LastEndPosition.Row, fTok.LastEndPosition.Col, lName.PositionPair.File);

                                    lSub = new PropertyAssignment(lPositionPair, lMode, (PropertyBaseExpression)lName, lValue);
                                }
                                lArgs.Add(lSub);
                                if (fTok.Token == TokenKind.Comma)
                                {
                                    fTok.Next();
                                    if (fTok.Token == TokenKind.CurlyClose) break;
                                }
                                else
                                if (fTok.Token == TokenKind.CurlyClose) break;
                                else
                                {
                                    Error(EcmaScriptErrorKind.ClosingBraceExpected, "");
                                    return null;
                                }
                            }
                        }
                        fTok.Next();
                        lVal = new ObjectLiteralExpression(new PositionPair(lPos, fTok.LastEndPosition), lArgs);
                    }
                    break;
                // Object Literal

                case TokenKind.Divide:
                    {
                        string lStr, lMod;
                        fTok.NextAsRegularExpression(out lStr, out lMod);
                        lVal = new RegExExpression(fTok.PositionPair, lStr, lMod);
                    }
                    break;
                case TokenKind.DivideAssign:
                    {
                        string lStr, lMod;
                        fTok.NextAsRegularExpression(out lStr, out lMod);
                        lVal = new RegExExpression(fTok.PositionPair, '=' + lStr, lMod);
                    }
                    break;
                case TokenKind.K_null:
                    {
                        lVal = new NullExpression(fTok.PositionPair);
                        fTok.Next();
                    }
                    break;
                case TokenKind.K_true:
                    {
                        lVal = new BooleanExpression(fTok.PositionPair, true);
                        fTok.Next();
                    }
                    break;
                case TokenKind.K_false:
                    {
                        lVal = new BooleanExpression(fTok.PositionPair, false);
                        fTok.Next();
                    }
                    break;
                case TokenKind.SingleQuoteString:
                case TokenKind.String:
                    {
                        lVal = new StringExpression(fTok.PositionPair, Tokenizer.DecodeString(fTok.TokenStr));
                        fTok.Next();
                    }
                    break;
                case TokenKind.Float:
                    {
                        var lValue = Utilities.ParseDouble(fTok.TokenStr);

                        if (Double.IsNaN(lValue))
                        {
                            Error(EcmaScriptErrorKind.SyntaxError, "");
                            return null;
                        }
                        lVal = new DecimalExpression(fTok.PositionPair, lValue);
                        fTok.Next();
                    }
                    break;
                case TokenKind.Number:
                    {
                        Int64 lValue;
                        if (!Int64.TryParse(fTok.TokenStr, out lValue))
                        {
                            var lDValue = Utilities.ParseDouble(fTok.TokenStr);

                            if (Double.IsNaN(lValue))
                            {
                                Error(EcmaScriptErrorKind.SyntaxError, "");
                                return null;
                            }
                            lVal = new DecimalExpression(fTok.PositionPair, lDValue);
                            fTok.Next();
                        }
                        else
                        {
                            lVal = new IntegerExpression(fTok.PositionPair, lValue);
                            fTok.Next();
                        }
                    }
                    break;
                case TokenKind.HexNumber:
                    {
                        Int64 lValue;
                        if (!Int64.TryParse(fTok.TokenStr.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo, out lValue))
                        {
                            Error(EcmaScriptErrorKind.SyntaxError, "");
                            return null;
                        }
                        lVal = new IntegerExpression(fTok.PositionPair, lValue);
                        fTok.Next();
                    }
                    break;
                case TokenKind.OpeningParenthesis:
                    {
                        fTok.Next();
                        lVal = ParseCommaExpression(true);
                        if (fTok.Token != TokenKind.ClosingParenthesis)
                        {
                            Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                            return null;
                        }
                        fTok.Next();
                    }
                    break;
                case TokenKind.K_new: {
                        var lPos = fTok.Position;
                        fTok.Next();
                        lVal = ParseLeftHandSideExpression(false);
                        if (lVal == null) return null;
                        var lArgs = new List<ExpressionElement>();
                        if (fTok.Token == TokenKind.OpeningParenthesis) {
                            fTok.Next();
                            if (fTok.Token != TokenKind.ClosingParenthesis) {
                                while (true) {
                                    var lSub = ParseExpression(true);
                                    if (lSub == null) return null;
                                    lArgs.Add(lSub);
                                    if (fTok.Token == TokenKind.Comma) fTok.Next();
                                    else
                                    if (fTok.Token == TokenKind.ClosingParenthesis) break;
                                    else
                                    {
                                        Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                                        return null;
                                    }
                                }
                            }
                            fTok.Next();
                        }
                        lVal = new NewExpression(new PositionPair(lPos, fTok.LastEndPosition), lVal, lArgs);
                    }
                    break;
                default:
                    {
                        Error(EcmaScriptErrorKind.SyntaxError, "");
                        return null;
                    }
            }

            while (fTok.Token == TokenKind.Dot || fTok.Token == TokenKind.OpeningBracket || fTok.Token == TokenKind.OpeningParenthesis) {
                if (fTok.Token == TokenKind.Dot) {
                    fTok.Next();
                    if (!(fTok.Token == TokenKind.K_set || fTok.Token == TokenKind.K_get || fTok.Token == TokenKind.Identifier)) {
                        Error(EcmaScriptErrorKind.IdentifierExpected, "");
                        return null;
                    }
                    lVal = new SubExpression(new PositionPair(lVal.PositionPair.StartPos, lVal.PositionPair.StartRow, lVal.PositionPair.StartCol, fTok.LastEndPosition.Pos, fTok.LastEndPosition.Row, fTok.LastEndPosition.Col, lVal.PositionPair.File), lVal, fTok.TokenStr);
                    fTok.Next();
                }
                else
                if (fTok.Token == TokenKind.OpeningBracket) {
                    fTok.Next();
                    var lSub = ParseExpression(true);
                    if (lSub == null) return null;
                    if (fTok.Token != TokenKind.ClosingBracket) {
                        Error(EcmaScriptErrorKind.ClosingBracketExpected, "");
                        return null;
                    }
                    fTok.Next();
                    lVal = new ArrayAccessExpression(new PositionPair(lVal.PositionPair.StartPos, lVal.PositionPair.StartRow, lVal.PositionPair.StartCol, fTok.LastEndPosition.Row, fTok.LastEndPosition.Pos, fTok.LastEndPosition.Col, lVal.PositionPair.File), lVal, lSub);
                } else { // opening parenthesis
                    if (!aParseParameters) break;
                    fTok.Next();
                    var lArgs = new List<ExpressionElement>();
                    if (fTok.Token != TokenKind.ClosingParenthesis) {
                        while (true) {
                            var lSub = ParseExpression(true);
                            if (lSub == null) return null;
                            lArgs.Add(lSub);
                            if (fTok.Token == TokenKind.Comma) fTok.Next();
                            else
                            if (fTok.Token == TokenKind.ClosingParenthesis) break;
                            else
                            {
                                Error(EcmaScriptErrorKind.ClosingParenthesisExpected, "");
                                return null;
                            }
                        }
                    }
                    fTok.Next();
                    lVal = new CallExpression(new PositionPair(lVal.PositionPair.StartPos, lVal.PositionPair.StartRow, lVal.PositionPair.StartCol, fTok.LastEndPosition.Pos, fTok.LastEndPosition.Row, fTok.LastEndPosition.Col, lVal.PositionPair.File), lVal, lArgs);
                }
            }
            return lVal;
        }

        public Parser()
        {
            fMessages = new List<ParserMessage>();
            fMessagesWrapper = new ReadOnlyCollection<ParserMessage>(fMessages);
        }

        public ReadOnlyCollection<ParserMessage> Messages
        {
            get
            {
                return fMessagesWrapper;
            }
        }

        public ProgramElement Parse(ITokenizer aTokenizer)
        {
            ProgramElement result;
            fTok = aTokenizer;
            fTok.Error += fTok_Error;
            try
            {
                fMessages.Clear();

                if (fTok.Token == TokenKind.EOF) return new ProgramElement(new PositionPair(fTok.Position, fTok.Position));
                var lPos = fTok.Position;
                var lItems = new List<SourceElement>();

                while (fTok.Token != TokenKind.EOF)
                {
                    var lItem = ParseStatement(ParseStatementFlags.AllowFunction);
                    if (lItem == null) break;
                    if (lItem.Type == ElementType.BlockStatement)
                        lItems.AddRange(((BlockStatement)lItem).Items);
                    else
                        lItems.Add(lItem);
                }

                result = new ProgramElement(new PositionPair(lPos, fTok.LastEndPosition), lItems);
            }
            finally
            {
                fTok.Error -= fTok_Error;
            }
            return result;
        }
    }
}
