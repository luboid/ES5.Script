using ES5.Script.EcmaScript.Bindings;
using ES5.Script.EcmaScript.Internal;
using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class EcmaScriptCompiler
    {
        internal EnvironmentRecord fRoot;
        internal string fLastData;

        GlobalObject fGlobal;
        bool fUseStrict;
        bool fStackProtect;
        bool fDebug;
        Label fExitLabel;
        LocalBuilder fExcept;
        LocalBuilder fResultVar;
        LocalBuilder fExecutionContext;
        ILGenerator fILG;
        List<LocalBuilder> fLocals;
        bool fJustFunctions;
        bool fDisableResult;
        List<Statement> fStatementStack;
        Label? fBreak;
        Label? fContinue;

        public GlobalObject GlobalObject
        {
            get
            {
                return fGlobal;
            }
        }

        public bool JustFunctions
        {
            get
            {
                return fJustFunctions;
            }
            set
            {
                fJustFunctions = value;
            }
        }


        public EcmaScriptCompiler(EcmaScriptCompilerOptions aOptions)
        {
            fGlobal = aOptions?.GlobalObject;
            if (aOptions != null)
            {
                fDebug = aOptions.EmitDebugCalls;
                fJustFunctions = aOptions.JustFunctions;
                fStackProtect = aOptions.StackOverflowProtect;
            }
            else
                fStackProtect = true;

            if (fGlobal == null) fGlobal = new GlobalObject(this);

            fRoot = new ObjectEnvironmentRecord(aOptions?.Context, fGlobal, false);
        }

        List<SourceElement> Parse(string aFilename, string aData, bool aEval = false)
        {
            var lTokenizer = new Tokenizer();
            var lParser = new Parser();

            lTokenizer.Error += lParser.fTok_Error;
            lTokenizer.SetData(aData, aFilename);
            lTokenizer.Error -= lParser.fTok_Error;

            fLastData = aData;
            var lElement = lParser.Parse(lTokenizer);
            foreach (var el in lParser.Messages)
            {
                if (el.IsError)
                    throw new ScriptParsingException(aFilename, new PositionPair(el.Position, el.Position), (EcmaScriptErrorKind)el.Code);
            }
            return lElement.Items;
        }

        public InternalDelegate EvalParse(bool aStrict, string aData)
        {
            var lSave = fLastData;
            var lUseStrict = fUseStrict;
            var lLoops = fStatementStack;
            try
            {
                fUseStrict = aStrict;
                var items = Parse("<eval>", aData, true);
                return (InternalDelegate)Parse(null, true, "<eval>", items);
            }
            finally
            {
                fLastData = lSave;
                fUseStrict = lUseStrict;
                fStatementStack = lLoops;
            }
        }

        public InternalDelegate Parse(string aFilename, string aData)
        {
            var lSave = fLastData;
            try
            {
                var items = Parse(aFilename, aData, false);
                return (InternalDelegate)Parse(null, false, aFilename, items);
            }
            finally
            {
                fLastData = lSave;
            }
        }

        public object Parse(FunctionDeclarationElement aFunction, bool aEval, string aScopeName, List<SourceElement> aElements)
        {
            if (aScopeName == null) aScopeName = "<anonymous>";

            var lLastData = fLastData;
            var lUseStrict = fUseStrict;
            var lLoops = fStatementStack;

            fStatementStack = new List<Statement>();
            var lOldDisableResults = fDisableResult;
            fDisableResult = aFunction != null;
            try
            {
                if (aElements.Count > 1)
                {
                    if ((aElements[0].Type == ElementType.ExpressionStatement) && (((ExpressionStatement)aElements[0]).ExpressionElement.Type == ElementType.StringExpression))
                    {
                        if (((StringExpression)((ExpressionStatement)aElements[0]).ExpressionElement).Value == "use strict")
                        {
                            fUseStrict = true;
                            aElements.RemoveAt(0);
                        }
                    }
                }

                var lOldLocals = fLocals;
                fLocals = new List<LocalBuilder>();
                var lMethod = (DynamicMethod)null;
#if SILVERLIGHT
                if (aFunction != null)
                    lMethod = new System.Reflection.Emit.DynamicMethod(aScopeName, typeof(Object), new[] { typeof(ExecutionContext), typeof(object), typeof(Object[]), typeof(EcmaScriptInternalFunctionObject) });
                else
                    lMethod = new System.Reflection.Emit.DynamicMethod(aScopeName, typeof(Object), new[] { typeof(ExecutionContext), typeof(object), typeof(Object[]) });
#else
                if (aFunction != null)
                    lMethod = new System.Reflection.Emit.DynamicMethod(aScopeName, typeof(Object), new[] { typeof(ExecutionContext), typeof(Object), typeof(object[]), typeof(EcmaScriptInternalFunctionObject) }, typeof(DynamicMethods), true);
                else
                    lMethod = new System.Reflection.Emit.DynamicMethod(aScopeName, typeof(Object), new[] { typeof(ExecutionContext), typeof(Object), typeof(object[]) }, typeof(DynamicMethods), true);
#endif
                var lOldBreak = fBreak;
                var lOldContinue = fContinue;
                fBreak = null;
                fContinue = null;
                var lOldILG = fILG;
                fILG = lMethod.GetILGenerator();
                var lOldExecutionContext = fExecutionContext;
                fExecutionContext = fILG.DeclareLocal(typeof(ExecutionContext));

                if (aFunction != null)
                {
                    fILG.Emit(OpCodes.Ldarg_0);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_LexicalScope);
                    fILG.Emit(OpCodes.Ldarg_0);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                    fILG.Emit(OpCodes.Newobj, DeclarativeEnvironmentRecord.ConstructorInfo);
                    fILG.Emit(OpCodes.Ldc_I4, fUseStrict ? 1 : 0);
                    fILG.Emit(OpCodes.Newobj, ExecutionContext.ConstructorInfo);
                    fILG.Emit(OpCodes.Stloc, fExecutionContext);

                    for (int i = aFunction.Parameters.Count - 1; i >= 0; i--)
                    {
                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                        fILG.Emit(OpCodes.Ldarg_2);
                        fILG.Emit(OpCodes.Ldc_I4, i);
                        fILG.Emit(OpCodes.Ldstr, aFunction.Parameters[i].Name);
                        fILG.Emit(OpCodes.Ldc_I4, fUseStrict ? 1 : 0);
                        fILG.Emit(OpCodes.Call, ExecutionContext.Method_StoreParameter);
                    }

                    // public delegate(aScope: ExecutionContext; aSelf: Object; params args: array of Object; aFunc: EcmaScriptInternalFunctionObject): Object;
                    // no need to check if it exists it is always local to the function scope
                    // it can be call with different number of parameters 

                    //fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    //fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_VariableScope);
                    //fILG.Emit(OpCodes.Ldstr, "arguments");
                    //fILG.Emit(OpCodes.Callvirt, EnvironmentRecord.Method_HasBinding);
                    //var lAlreadyHaveArguments = fILG.DefineLabel();
                    //fILG.Emit(OpCodes.Brtrue, lAlreadyHaveArguments);

                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Ldarg_2);
                    fILG.Emit(OpCodes.Ldc_I4, aFunction.Parameters.Count);
                    fILG.Emit(OpCodes.Newarr, typeof(String));
                    for (int i = 0, l = aFunction.Parameters.Count; i < l; i++)
                    {
                        fILG.Emit(OpCodes.Dup);
                        fILG.Emit(OpCodes.Ldc_I4, i);
                        fILG.Emit(OpCodes.Ldstr, aFunction.Parameters[i].Name);
                        fILG.Emit(OpCodes.Stelem_Ref);
                    }

                    fILG.Emit(OpCodes.Ldarg, 3);
                    //eecution context, object[], function
                    fILG.Emit(OpCodes.Ldc_I4, fUseStrict ? 1 : 0);
                    fILG.Emit(OpCodes.Newobj, EcmaScriptArgumentObject.ConstructorInfo);

                    fILG.Emit(OpCodes.Ldstr, "arguments");
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_VariableScope);

                    fILG.Emit(OpCodes.Ldc_I4, fUseStrict ? 11 : 0);
                    fILG.Emit(OpCodes.Ldc_I4_0);
                    fILG.Emit(OpCodes.Call, EnvironmentRecord.Method_CreateAndSetMutableBindingNoFail);

                    //fILG.MarkLabel(lAlreadyHaveArguments);

                    var lHaveThis = fILG.DefineLabel();
                    var lHaveNoThis = fILG.DefineLabel();
                    fILG.Emit(OpCodes.Ldarg_1);
                    fILG.Emit(OpCodes.Call, Undefined.Method_Instance);
                    fILG.Emit(OpCodes.Beq, lHaveNoThis);
                    fILG.Emit(OpCodes.Ldarg_1);
                    fILG.Emit(OpCodes.Brfalse, lHaveNoThis);
                    fILG.Emit(OpCodes.Br, lHaveThis);
                    fILG.MarkLabel(lHaveNoThis);
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                    fILG.Emit(OpCodes.Starg, 1);
                    fILG.MarkLabel(lHaveThis);
                }
                else
                {
                    fILG.Emit(OpCodes.Ldarg_0);  // first execution context
                    fILG.Emit(OpCodes.Stloc, fExecutionContext);
                    if (!aEval)
                    {
                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                        fILG.Emit(OpCodes.Ldc_I4, fUseStrict ? 1 : 0);
                        fILG.Emit(OpCodes.Call, ExecutionContext.method_SetStrict);
                    }
                }

                if (fDebug)
                {
                    WriteDebugStack();
                    fILG.Emit(OpCodes.Ldstr, aScopeName);
                    fILG.Emit(OpCodes.Ldarg, 1); // this
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Callvirt, DebugSink.Method_EnterScope);
                }
                if (fStackProtect)
                {
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                    fILG.Emit(OpCodes.Call, GlobalObject.Method_IncreaseFrame);
                }

                if (fDebug) fILG.BeginExceptionBlock(); // filter
                fILG.BeginExceptionBlock(); // finally

                if (fDebug && !aEval && (aFunction == null))
                    fILG.BeginExceptionBlock(); // except

                var lOldExitLabel = fExitLabel;
                var lOldResultVar = fResultVar;
                var lOldExcept = fExcept;
                if (fDebug)
                {
                    fExcept = fILG.DeclareLocal(typeof(Boolean));
                    fILG.Emit(OpCodes.Ldc_I4_0);
                    fILG.Emit(OpCodes.Stloc, fExcept);
                }

                fExitLabel = fILG.DefineLabel();
                fResultVar = fILG.DeclareLocal(typeof(Object));
                fILG.Emit(OpCodes.Call, Undefined.Method_Instance);
                fILG.Emit(OpCodes.Stloc, fResultVar);

                if (!aEval && (aFunction == null))
                {
                    fILG.Emit(OpCodes.Ldarg_1); // this
                    var lIsNull = fILG.DefineLabel();
                    fILG.Emit(OpCodes.Brfalse, lIsNull);
                    fILG.Emit(OpCodes.Ldarg_1); // this
                    fILG.Emit(OpCodes.Call, Undefined.Method_Instance);
                    fILG.Emit(OpCodes.Beq, lIsNull);
                    var lGotThis = fILG.DefineLabel();
                    fILG.Emit(OpCodes.Br, lGotThis);
                    fILG.MarkLabel(lIsNull);
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                    fILG.Emit(OpCodes.Starg, 1); // this
                    fILG.MarkLabel(lGotThis);
                }

                DefineInScope(aEval, aElements);

                var lJustFunction = fJustFunctions && (aFunction == null) && !aEval;

                for (int i = 0, l = aElements.Count; i < l; i++)
                {
                    if (!lJustFunction || (aElements[i].Type == ElementType.FunctionDeclaration))
                        WriteStatement(aElements[i]);
                }

                if (fDebug)
                { // filter
                    fILG.BeginCatchBlock(typeof(Object));
                    fILG.Emit(OpCodes.Stloc, fResultVar);
                    fILG.Emit(OpCodes.Ldc_I4_1);
                    fILG.Emit(OpCodes.Stloc, fExcept);
                    fILG.Emit(OpCodes.Rethrow);
                    fILG.EndExceptionBlock();
                }

                fILG.BeginFinallyBlock();
                if (fStackProtect)
                {
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                    fILG.Emit(OpCodes.Call, GlobalObject.Method_DecreaseFrame);
                }

                if (fDebug)
                {
                    WriteDebugStack();
                    fILG.Emit(OpCodes.Ldstr, aScopeName);
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Ldloc, fResultVar);
                    fILG.Emit(OpCodes.Ldloc, fExcept);
                    fILG.Emit(OpCodes.Callvirt, DebugSink.Method_ExitScope);
                }

                fILG.EndExceptionBlock();

                if (fDebug)
                {
                    if (!aEval && (aFunction == null))
                    {
                        fILG.BeginCatchBlock(typeof(Exception));
                        var lTemp = AllocateLocal(typeof(Exception));
                        fILG.Emit(OpCodes.Stloc, lTemp);
                        WriteDebugStack();
                        fILG.Emit(OpCodes.Ldloc, lTemp);
                        fILG.Emit(OpCodes.Callvirt, DebugSink.Method_UncaughtException);
                        fILG.Emit(OpCodes.Rethrow);
                        ReleaseLocal(lTemp);
                        fILG.EndExceptionBlock();
                    }
                }

                fILG.MarkLabel(fExitLabel);
                fILG.Emit(OpCodes.Ldloc, fResultVar);
                fILG.Emit(OpCodes.Ret);


                fExcept = lOldExcept;
                fExitLabel = lOldExitLabel;
                fResultVar = lOldResultVar;
                fILG = lOldILG;
                fExecutionContext = lOldExecutionContext;
                fLocals = lOldLocals;
                fBreak = lOldBreak;
                fContinue = lOldContinue;
                if (aFunction != null)
                    return lMethod.CreateDelegate(typeof(InternalFunctionDelegate));
                return lMethod.CreateDelegate(typeof(InternalDelegate));
            }
            finally
            {
                fDisableResult = lOldDisableResults;
                fUseStrict = lUseStrict;
                fStatementStack = lLoops;
                fLastData = lLastData;
            }
        }

        void WriteDebugStack()
        {
            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
            fILG.Emit(OpCodes.Call, ExecutionContext.Method_GetDebugSink);
        }

        void WriteStatement(SourceElement El)
        {
            if (El == null) return;
            if (fDebug && (El.PositionPair.StartRow > 0))
            {
                WriteDebugStack();
                var lPos = El.PositionPair;
                fILG.Emit(OpCodes.Ldstr, lPos.File);
                fILG.Emit(OpCodes.Ldc_I4, lPos.StartRow);
                fILG.Emit(OpCodes.Ldc_I4, lPos.StartCol);
                fILG.Emit(OpCodes.Ldc_I4, lPos.EndRow);
                fILG.Emit(OpCodes.Ldc_I4, lPos.EndCol);
                fILG.Emit(OpCodes.Callvirt, DebugSink.Method_DebugLine);
            }

            if (El is Statement)
                fStatementStack.Add((Statement)El);

            switch (El.Type)
            {
                case ElementType.EmptyStatement:
                    fILG.Emit(OpCodes.Nop);
                    break;

                case ElementType.ReturnStatement:
                    {
                        if (!fDisableResult) throw new ScriptParsingException(El.PositionPair.File, El.PositionPair, EcmaScriptErrorKind.CannotReturnHere);
                        var e = (ReturnStatement)El;
                        if (e.ExpressionElement == null)
                            fILG.Emit(OpCodes.Call, Undefined.Method_Instance);
                        else
                        {
                            WriteExpression(e.ExpressionElement);
                            CallGetValue(e.ExpressionElement.Type);
                        }
                        fILG.Emit(OpCodes.Stloc, fResultVar);
                        var lFinallyInfo = Enumerable.Reverse(fStatementStack)
                                              .Where(a => ((a.Type == ElementType.TryStatement) && ((TryStatement)a).FinallyData != null))
                                              .Select(a => ((TryStatement)a).FinallyData).ToArray();

                        if (lFinallyInfo.Length > 0)
                        {
                            for (int i = 0, l = lFinallyInfo.Length; i < l; i++)
                            {
                                fILG.Emit(OpCodes.Ldc_I4, lFinallyInfo[i].AddUnique(i < lFinallyInfo.Length - 1 ? lFinallyInfo[i + 1].FinallyLabel : fExitLabel));
                                fILG.Emit(OpCodes.Stloc, lFinallyInfo[i].FinallyState);
                            }

                            if (((TryStatement)Enumerable.Reverse(fStatementStack).FirstOrDefault(a => a.Type == ElementType.TryStatement)).Catch != null)
                                fILG.Emit(OpCodes.Leave, lFinallyInfo[0].FinallyLabel);
                            else
                                fILG.Emit(OpCodes.Br, lFinallyInfo[0].FinallyLabel);
                        }
                        else
                            fILG.Emit(OpCodes.Leave, fExitLabel); // there"s always an outside finally
                    }
                    break;
                case ElementType.ExpressionStatement:
                    {
                        var e = ((ExpressionStatement)El).ExpressionElement;
                        WriteExpression(e);
                        CallGetValue(e.Type);
                        if (fDisableResult)
                            fILG.Emit(OpCodes.Pop);
                        else
                            fILG.Emit(OpCodes.Stloc, fResultVar);
                    }
                    break;
                case ElementType.DebuggerStatement:
                    {
                        WriteDebugStack();
                        fILG.Emit(OpCodes.Callvirt, DebugSink.Method_Debugger);
                    }
                    break;
                case ElementType.VariableStatement:
                    {
                        var items = ((VariableStatement)El).Items;
                        for (int i = 0, l = items.Count; i < l; i++)
                        {
                            var lItem = items[i];
                            if (lItem.Initializer != null)
                            {
                                WriteExpression(new BinaryExpression(lItem.PositionPair, new IdentifierExpression(lItem.PositionPair, lItem.Identifier), lItem.Initializer, BinaryOperator.Assign));
                                fILG.Emit(OpCodes.Pop);
                            }
                        }
                    }
                    break;
                case ElementType.BlockStatement:
                    {
                        foreach (var subitem in ((BlockStatement)El).Items)
                            WriteStatement(subitem);
                    }
                    break;
                case ElementType.IfStatement: WriteIfStatement(((IfStatement)El)); break;
                case ElementType.BreakStatement: WriteBreak(((BreakStatement)El)); break;
                case ElementType.ContinueStatement: WriteContinue(((ContinueStatement)El)); break;
                case ElementType.DoStatement: WriteDoStatement(((DoStatement)El)); break;
                case ElementType.ForInStatement: WriteForInstatement((ForInStatement)El); break;
                case ElementType.ForStatement: WriteForStatement(((ForStatement)El)); break;
                case ElementType.WhileStatement: WriteWhileStatement(((WhileStatement)El)); break;
                case ElementType.LabelledStatement:
                    {
                        var lWas = fILG.DefineLabel();
                        var l = (LabelledStatement)El;
                        l.Break = lWas;
                        WriteStatement(l.Statement);
                        if (lWas == (Label)l.Break)
                            fILG.MarkLabel((Label)l.Break);
                    }
                    break;
                case ElementType.FunctionDeclaration:
                    {
                        var l = (FunctionDeclarationElement)El;
                        if (l.Identifier == null)
                        {
                            WriteFunction(l, false);
                            if (fDisableResult)
                                fILG.Emit(OpCodes.Pop);
                            else
                                fILG.Emit(OpCodes.Stloc, fResultVar);
                        }
                    }
                    break;
                case ElementType.ThrowStatement:
                    {
                        var l = (ThrowStatement)El;
                        WriteExpression(l.ExpressionElement);
                        CallGetValue(l.ExpressionElement.Type);
                        fILG.Emit(OpCodes.Call, ScriptRuntimeException.Method_Wrap);
                        fILG.Emit(OpCodes.Throw);
                    }
                    break;
                case ElementType.WithStatement:
                    {
                        if (fUseStrict)
                            throw new ScriptParsingException(El.PositionPair.File, El.PositionPair, EcmaScriptErrorKind.WithNotAllowedInStrict);

                        WriteWithStatement((WithStatement)El);
                    }
                    break;
                case ElementType.TryStatement:
                    WriteTryStatement((TryStatement)El);
                    break;
                case ElementType.SwitchStatement:
                    WriteSwitchstatement((SwitchStatement)El);
                    break;
                default:
                    throw new EcmaScriptException(El.PositionPair.File, El.PositionPair, EcmaScriptErrorKind.EInternalError, "Unkwown type: " + El.Type);
            } // case

            fStatementStack.Remove(El as Statement);
        }

        LocalBuilder AllocateLocal(Type aType)
        {
            for (int i = 0, l = fLocals.Count; i < l; i++)
            {
                if (fLocals[i].LocalType == aType)
                {
                    var lItem = fLocals[i];
                    fLocals.RemoveAt(i);
                    return lItem;
                }
            }
            return fILG.DeclareLocal(aType);
        }

        void ReleaseLocal(LocalBuilder aLocal)
        {
            fLocals.Add(aLocal);
        }

        static readonly ElementType[] callGetValueElement = new[] { ElementType.SubExpression, ElementType.CallExpression, ElementType.ArrayAccessExpression, ElementType.IdentifierExpression };
        void CallGetValue(ElementType elementType)
        {
            if (-1 == Array.IndexOf<ElementType>(callGetValueElement, elementType))
                return;

            // Expect: POSSIBLE reference on stack (always typed object)
            // Returns: Object value
            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
            fILG.Emit(OpCodes.Call, Reference.Method_GetValue);
        }

        void CallSetValue()
        {
            // Expect: POSSIBLE reference on stack (always typed object)
            // Expect: NON reference as second item on the stack
            // Returns: Object value
            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
            fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
        }

        void MarkLabelled(Label? aBreak, Label? aContinue)
        {
            var lLoopItem = (IterationStatement)fStatementStack[fStatementStack.Count - 1];
            if (lLoopItem != null)
            {
                lLoopItem.Break = aBreak;
                lLoopItem.Continue = aContinue;
            }

            for (int i = fStatementStack.Count - 2; i >= 0; i--)
            { // -1 = current
                var lItem = fStatementStack[i] as LabelledStatement;
                if (lItem == null) return;
                lItem.Break = aBreak ?? lItem.Break;
                lItem.Continue = aContinue ?? lItem.Continue;
            }
        }

        void WriteFunction(FunctionDeclarationElement el, bool aRegister)
        {
            var lDelegate = (InternalFunctionDelegate)Parse(el, false, el.Identifier, el.Items);
            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
            fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
            if (el.Identifier == null)
            {
                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_LexicalScope);
            }
            else
            {
                if (aRegister)
                {
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_VariableScope);
                }
                else
                {
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_LexicalScope);
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                    fILG.Emit(OpCodes.Newobj, DeclarativeEnvironmentRecord.ConstructorInfo);
                }
            }

            if (el.Identifier == null)
                fILG.Emit(OpCodes.Ldnull);
            else
                fILG.Emit(OpCodes.Ldstr, el.Identifier);

            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
            fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
            fILG.Emit(OpCodes.Ldc_I4, fGlobal.StoreFunction(lDelegate));
            fILG.Emit(OpCodes.Call, GlobalObject.Method_GetFunction);

            fILG.Emit(OpCodes.Ldc_I4, el.Parameters.Count);
            var ob = GetOriginalBody(el);
            fILG.Emit(OpCodes.Ldstr, ob);
            fILG.Emit(OpCodes.Ldc_I4, fUseStrict ? 1 : 0);
            fILG.Emit(OpCodes.Newobj, EcmaScriptInternalFunctionObject.ConstructorInfo);


            if ((el.Identifier != null) && !aRegister)
            {
                //class method SetAndInitializeImmutable(val: EcmaScriptFunctionObject; aName: String): EcmaScriptFunctionObject;
                fILG.Emit(OpCodes.Ldstr, el.Identifier);
                fILG.Emit(OpCodes.Call, DeclarativeEnvironmentRecord.Method_SetAndInitializeImmutable);
            }
        }

        void WriteDoStatement(DoStatement el)
        {
            var lOldContinue = fContinue;
            var lOldBreak = fBreak;
            fContinue = fILG.DefineLabel();
            fBreak = fILG.DefineLabel();
            MarkLabelled(fBreak, fContinue);
            var fRestart = fILG.DefineLabel();

            fILG.MarkLabel(fRestart);

            WriteStatement(el.Body);

            fILG.MarkLabel((Label)fContinue);

            WriteExpression(el.ExpressionElement);
            CallGetValue(el.ExpressionElement.Type);
            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
            fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
            fILG.Emit(OpCodes.Brtrue, (Label)fRestart);
            fILG.MarkLabel((Label)fBreak);

            fBreak = lOldBreak;
            fContinue = lOldContinue;
        }


        void WriteWhileStatement(WhileStatement el)
        {
            var lOldContinue = fContinue;
            var lOldBreak = fBreak;
            fContinue = fILG.DefineLabel();
            fBreak = fILG.DefineLabel();
            MarkLabelled(fBreak, fContinue);
            fILG.MarkLabel((Label)fContinue);
            WriteExpression(el.ExpressionElement);
            CallGetValue(el.ExpressionElement.Type);
            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
            fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
            fILG.Emit(OpCodes.Brfalse, (Label)fBreak);

            WriteStatement(el.Body);

            fILG.Emit(OpCodes.Br, (Label)fContinue);
            fILG.MarkLabel((Label)fBreak);

            fBreak = lOldBreak;
            fContinue = lOldContinue;
        }

        void WriteForInstatement(ForInStatement el)
        {
            var lLocal = AllocateLocal(typeof(IEnumerator<String>));
            var lOldContinue = fContinue;
            var lOldBreak = fBreak;
            fContinue = fILG.DefineLabel();

            var lCurrent = lLocal.LocalType.GetMethod("get_Current");
            var lMoveNext = typeof(System.Collections.IEnumerator).GetMethod("MoveNext");
            fBreak = fILG.DefineLabel();

            var lWork = ((el.Initializer?.Identifier) != null) ? new IdentifierExpression(el.Initializer.PositionPair, el.Initializer.Identifier) :
                      el.LeftExpression;

            WriteExpression(el.ExpressionElement);
            CallGetValue(el.ExpressionElement.Type);
            fILG.Emit(OpCodes.Isinst, typeof(EcmaScriptObject));
            fILG.Emit(OpCodes.Dup);
            var lPopAndBreak = fILG.DefineLabel();
            fILG.Emit(OpCodes.Brfalse, lPopAndBreak);
            fILG.Emit(OpCodes.Callvirt, EcmaScriptObject.Method_GetNames);
            fILG.Emit(OpCodes.Stloc, lLocal);

            MarkLabelled(fBreak, fContinue);

            fILG.MarkLabel((Label)fContinue);
            fILG.Emit(OpCodes.Ldloc, lLocal);
            fILG.Emit(OpCodes.Callvirt, lMoveNext);
            fILG.Emit(OpCodes.Brfalse, (Label)fBreak);

            WriteExpression(lWork);
            fILG.Emit(OpCodes.Ldloc, lLocal);
            fILG.Emit(OpCodes.Callvirt, lCurrent);
            CallSetValue();
            fILG.Emit(OpCodes.Pop); // set value returns something

            WriteStatement(el.Body);

            fILG.Emit(OpCodes.Br, (Label)fContinue);
            fILG.MarkLabel(lPopAndBreak);
            fILG.Emit(OpCodes.Pop);
            fILG.MarkLabel((Label)fBreak);

            fBreak = lOldBreak;
            fContinue = lOldContinue;

            ReleaseLocal(lLocal);
        }

        void WriteForStatement(ForStatement el)
        {
            var lOldContinue = fContinue;
            var lOldBreak = fBreak;
            fContinue = fILG.DefineLabel();
            fBreak = fILG.DefineLabel();
            MarkLabelled(fBreak, fContinue);

            if (el.Initializer != null)
            {
                WriteExpression(el.Initializer);
                fILG.Emit(OpCodes.Pop);
            }

            foreach (var eli in el.Initializers)
            {
                if (eli.Initializer != null)
                {
                    WriteExpression(new BinaryExpression(eli.PositionPair,
                        new IdentifierExpression(eli.PositionPair, eli.Identifier), eli.Initializer, BinaryOperator.Assign));
                    fILG.Emit(OpCodes.Pop);
                }
            }

            var lLoopStart = fILG.DefineLabel();
            fILG.MarkLabel(lLoopStart);
            if (el.Comparison != null)
            {
                WriteExpression(el.Comparison);
                CallGetValue(el.Comparison.Type);
                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
                fILG.Emit(OpCodes.Brfalse, (Label)fBreak);
            }

            WriteStatement(el.Body);

            fILG.MarkLabel((Label)fContinue);

            if (el.Increment != null)
            {
                WriteExpression(el.Increment);
                fILG.Emit(OpCodes.Pop);
            }

            fILG.Emit(OpCodes.Br, lLoopStart);

            fILG.MarkLabel((Label)fBreak);

            fBreak = lOldBreak;
            fContinue = lOldContinue;
        }

        void WriteWithStatement(WithStatement el)
        {
            var lNew = AllocateLocal(typeof(ExecutionContext));
            var lOld = fExecutionContext;

            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
            WriteExpression(el.ExpressionElement);
            CallGetValue(el.ExpressionElement.Type);
            fILG.Emit(OpCodes.Call, ExecutionContext.Method_With);
            fILG.Emit(OpCodes.Stloc, lNew);
            fExecutionContext = lNew;
            WriteStatement(el.Body);
            fExecutionContext = lOld;
            ReleaseLocal(lOld);
        }

        void WriteTryStatement(TryStatement el)
        {
            if (el.Finally != null)
            {
                el.FinallyData = new FinallyInfo();
                el.FinallyData.FinallyLabel = fILG.DefineLabel();
                el.FinallyData.FinallyState = AllocateLocal(typeof(int));
                fILG.BeginExceptionBlock();
            }

            if (el.Catch != null) fILG.BeginExceptionBlock();

            if (el.Body != null) WriteStatement(el.Body);

            if (el.Catch != null)
            {
                fILG.BeginCatchBlock(typeof(Exception));
                fILG.Emit(OpCodes.Call, ScriptRuntimeException.Method_Unwrap);
                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                fILG.Emit(OpCodes.Ldstr, el.Catch.Identifier);
                fILG.Emit(OpCodes.Call, ExecutionContext.Method_Catch);
                var lVar = AllocateLocal(fExecutionContext.GetType());
                fILG.Emit(OpCodes.Stloc, lVar);
                var lold = fExecutionContext;
                fExecutionContext = lVar;
                //DefineInScope(false, [el.Catch.Body]);
                WriteStatement(el.Catch.Body);
                ReleaseLocal(lVar);
                fExecutionContext = lold;
                fILG.EndExceptionBlock();
            }

            if (el.Finally != null)
            {
                var lData = el.FinallyData;
                el.FinallyData = null;
                fILG.Emit(OpCodes.Ldc_I4_M1);
                fILG.Emit(OpCodes.Stloc, lData.FinallyState);
                fILG.MarkLabel(lData.FinallyLabel);
                var lOldDisableResult = fDisableResult;
                fDisableResult = true;
                WriteStatement(el.Finally);
                fILG.Emit(OpCodes.Ldloc, lData.FinallyState);
                fILG.Emit(OpCodes.Switch, lData.JumpTable.ToArray());
                fILG.BeginCatchBlock(typeof(Exception));
                fILG.Emit(OpCodes.Pop);
                WriteStatement(el.Finally);
                fILG.Emit(OpCodes.Rethrow);
                fILG.EndExceptionBlock();
                fDisableResult = lOldDisableResult;
            }
        }

        void WriteSwitchstatement(SwitchStatement el)
        {
            var lLabels = new Label[el.Clauses.Count];
            for (int i = 0, l = lLabels.Length; i < l; i++)
            {
                lLabels[i] = fILG.DefineLabel();
            }

            var lWork = AllocateLocal(typeof(object));
            WriteExpression(el.ExpressionElement);
            CallGetValue(el.ExpressionElement.Type);
            fILG.Emit(OpCodes.Stloc, lWork);

            var lGotDefault = false;
            for (int i = 0, l = el.Clauses.Count; i < l; i++)
            {
                if (el.Clauses[i].ExpressionElement == null)
                {
                    lGotDefault = true;
                }
                else
                {
                    fILG.Emit(OpCodes.Ldloc, lWork);
                    WriteExpression(el.Clauses[i].ExpressionElement);
                    CallGetValue(el.Clauses[i].ExpressionElement.Type);
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, Operators.Method_StrictEqual);
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
                    fILG.Emit(OpCodes.Brtrue, lLabels[i]);
                }
            }
            ReleaseLocal(lWork);

            var lOldContinue = fContinue;
            var lOldBreak = fBreak;
            fBreak = fILG.DefineLabel();
            if (lGotDefault)
            {
                for (int i = 0, l = el.Clauses.Count; i < l; i++)
                {
                    if (el.Clauses[i].ExpressionElement == null)
                    {
                        fILG.Emit(OpCodes.Br, lLabels[i]);
                        break;
                    }
                }
            }
            else
            {
                fILG.Emit(OpCodes.Br, (Label)fBreak);
            }
            MarkLabelled(fBreak, null);
            if (!lGotDefault) fILG.Emit(OpCodes.Br, (Label)fBreak);


            for (int i = 0, l = el.Clauses.Count; i < l; i++)
            {
                fILG.MarkLabel(lLabels[i]);
                foreach (var bodyelement in el.Clauses[i].Body)
                    WriteStatement(bodyelement);
            }

            fILG.MarkLabel((Label)fBreak);

            fBreak = lOldBreak;
            fContinue = lOldContinue;
        }


        void DefineInScope(bool aEval, IEnumerable<SourceElement> aElements)
        {
            foreach (var el in RecursiveFindFuncAndVars(aElements))
            {
                var e = el as FunctionDeclarationElement;
                if ((el.Type == ElementType.FunctionDeclaration) && (e != null) && (e.Identifier != null))
                {
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_VariableScope);
                    fILG.Emit(OpCodes.Ldstr, e.Identifier);
                    if (aEval)
                        fILG.Emit(OpCodes.Ldc_I4_1);
                    else
                        fILG.Emit(OpCodes.Ldc_I4_0);
                    fILG.Emit(OpCodes.Call, EnvironmentRecord.Method_CreateMutableBindingNoFail);
                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                    fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_VariableScope);
                    fILG.Emit(OpCodes.Ldstr, e.Identifier);
                    WriteFunction(e, true);
                    if (fUseStrict)
                        fILG.Emit(OpCodes.Ldc_I4_1);
                    else
                        fILG.Emit(OpCodes.Ldc_I4_0);
                    fILG.Emit(OpCodes.Callvirt, EnvironmentRecord.Method_SetMutableBinding);
                }
                else if (el.Type == ElementType.VariableStatement)
                {
                    foreach (var en in ((VariableStatement)el).Items)
                    {
                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                        fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_VariableScope);
                        fILG.Emit(OpCodes.Ldstr, en.Identifier);
                        if (aEval)
                            fILG.Emit(OpCodes.Ldc_I4_1);
                        else
                            fILG.Emit(OpCodes.Ldc_I4_0);
                        fILG.Emit(OpCodes.Call, EnvironmentRecord.Method_CreateMutableBindingNoFail);
                    }
                }
            }
        }

        string GetOriginalBody(SourceElement el)
        {
            var lStart = el.PositionPair.StartPos;
            var lEnd = el.PositionPair.EndPos - lStart;
            if ((lStart >= 0) && (lStart + lEnd < fLastData.Length))
                return fLastData.Substring(lStart, lEnd);
            else
                return "{}";
        }

        void WriteBreak(BreakStatement el)
        {
            var lPassedTry = false;
            List<FinallyInfo> lFinallyInfo = null;
            if (el.Identifier == null)
            {
                if (fBreak == null)
                    throw new ScriptParsingException(el.PositionPair.File, el.PositionPair, EcmaScriptErrorKind.CannotBreakHere);

                for (var i = fStatementStack.Count - 1; i >= 0; i--)
                {
                    if (fStatementStack[i].Type == ElementType.TryStatement)
                    {
                        lPassedTry = true;
                        if (((TryStatement)fStatementStack[i]).FinallyData != null)
                        {
                            if (lFinallyInfo == null) lFinallyInfo = new List<FinallyInfo>();
                            lFinallyInfo.Add(((TryStatement)fStatementStack[i]).FinallyData);
                        }
                    }
                    else
                        if (((IterationStatement)fStatementStack[i])?.Break == fBreak)
                    {
                        break;
                    }
                }

                if (lPassedTry)
                {
                    if (lFinallyInfo != null)
                    {
                        for (int i = 0, l = lFinallyInfo.Count; i < l; i++)
                        {
                            fILG.Emit(OpCodes.Ldc_I4, lFinallyInfo[i].AddUnique(i < lFinallyInfo.Count - 1 ? lFinallyInfo[i + 1].FinallyLabel : (Label)fBreak));
                            fILG.Emit(OpCodes.Stloc, lFinallyInfo[i].FinallyState);
                        }
                        fILG.Emit(OpCodes.Br, lFinallyInfo[0].FinallyLabel);
                    }
                    else
                        fILG.Emit(OpCodes.Leave, (Label)fBreak);
                }
                else
                    fILG.Emit(OpCodes.Br, (Label)fBreak);
            }
            else
            {
                for (var i = fStatementStack.Count - 1; i >= 0; i--)
                {
                    var lIt = (LabelledStatement)fStatementStack[i];
                    if (fStatementStack[i].Type == ElementType.TryStatement)
                    {
                        lPassedTry = true;
                        if (((TryStatement)fStatementStack[i]).Finally != null)
                        {
                            if (lFinallyInfo == null) lFinallyInfo = new List<FinallyInfo>();
                            lFinallyInfo.Add(((TryStatement)fStatementStack[i]).FinallyData);
                        }
                    }
                    else if ((lIt != null) && (lIt.Identifier == el.Identifier))
                    {
                        if (lPassedTry)
                        {
                            if (lFinallyInfo != null)
                            {
                                for (int j = 0, l = lFinallyInfo.Count; i < l; i++)
                                {
                                    fILG.Emit(OpCodes.Ldc_I4, lFinallyInfo[j].AddUnique(j < lFinallyInfo.Count - 1 ? lFinallyInfo[j + 1].FinallyLabel : (Label)lIt.Break));
                                    fILG.Emit(OpCodes.Stloc, lFinallyInfo[j].FinallyState);
                                }
                                fILG.Emit(OpCodes.Br, lFinallyInfo[0].FinallyLabel);
                            }
                            else
                                fILG.Emit(OpCodes.Leave, (Label)lIt.Break);
                        }
                        else
                        {
                            fILG.Emit(OpCodes.Leave, (Label)lIt.Break);
                        }
                        return;
                    }
                }
                throw new ScriptParsingException(el.PositionPair.File, el.PositionPair, EcmaScriptErrorKind.UnknownLabelTarget, el.Identifier);
            }
        }

        void WriteContinue(ContinueStatement el)
        {
            var lPassedTry = false;
            List<FinallyInfo> lFinallyInfo = null;
            if (el.Identifier == null)
            {
                if (fContinue == null)
                    throw new ScriptParsingException(el.PositionPair.File, el.PositionPair, EcmaScriptErrorKind.CannotContinueHere);

                for (var i = fStatementStack.Count - 1; i >= 0; i--)
                {
                    if (fStatementStack[i].Type == ElementType.TryStatement)
                    {
                        lPassedTry = true;
                        if (((TryStatement)fStatementStack[i]).FinallyData != null)
                        {
                            if (lFinallyInfo == null) lFinallyInfo = new List<FinallyInfo>();
                            lFinallyInfo.Add(((TryStatement)fStatementStack[i]).FinallyData);
                        }
                    }
                    else if (((IterationStatement)fStatementStack[i])?.Continue == fContinue)
                    {
                        break;
                    }
                }

                if (lPassedTry)
                {
                    if (lFinallyInfo != null)
                    {
                        for (int i = 0, l = lFinallyInfo.Count; i < l; i++)
                        {
                            fILG.Emit(OpCodes.Ldc_I4, lFinallyInfo[i].AddUnique(i < lFinallyInfo.Count - 1 ? lFinallyInfo[i + 1].FinallyLabel : (Label)fContinue));
                            fILG.Emit(OpCodes.Stloc, lFinallyInfo[i].FinallyState);
                        }
                        if (((TryStatement)Enumerable.Reverse(fStatementStack).FirstOrDefault(a => a.Type == ElementType.TryStatement)).Catch != null)
                            fILG.Emit(OpCodes.Leave, lFinallyInfo[0].FinallyLabel);
                        else
                            fILG.Emit(OpCodes.Br, lFinallyInfo[0].FinallyLabel);
                    }
                    else
                        fILG.Emit(OpCodes.Leave, (Label)fContinue);
                }
                else
                    fILG.Emit(OpCodes.Br, (Label)fContinue);
            }
            else
            {
                for (var i = fStatementStack.Count - 1; i >= 0; i--)
                {
                    var lIt = (LabelledStatement)fStatementStack[i];
                    if (fStatementStack[i].Type == ElementType.TryStatement)
                    {
                        lPassedTry = true;
                        if (((TryStatement)fStatementStack[i]).Finally != null)
                        {
                            if (lFinallyInfo == null) lFinallyInfo = new List<FinallyInfo>();
                            lFinallyInfo.Add(((TryStatement)fStatementStack[i]).FinallyData);
                        }
                    }
                    else if ((lIt != null) && (lIt.Identifier == el.Identifier))
                    {
                        if (lPassedTry)
                        {
                            if (lFinallyInfo != null)
                            {
                                for (int j = 0, l = lFinallyInfo.Count; i < l; i++)
                                {
                                    fILG.Emit(OpCodes.Ldc_I4, lFinallyInfo[j].AddUnique(j < lFinallyInfo.Count - 1 ? lFinallyInfo[j + 1].FinallyLabel : (Label)lIt.Continue));
                                    fILG.Emit(OpCodes.Stloc, lFinallyInfo[j].FinallyState);
                                }
                                if (((TryStatement)Enumerable.Reverse(fStatementStack).FirstOrDefault(a => a.Type == ElementType.TryStatement)).Catch != null)
                                    fILG.Emit(OpCodes.Leave, lFinallyInfo[0].FinallyLabel);
                                else
                                    fILG.Emit(OpCodes.Br, lFinallyInfo[0].FinallyLabel);
                            }
                            else
                                fILG.Emit(OpCodes.Leave, (Label)lIt.Continue);
                        }
                        else
                        {
                            if (lIt.Continue == null)
                                fILG.Emit(OpCodes.Leave, fExitLabel);
                            else
                                fILG.Emit(OpCodes.Leave, (Label)lIt.Continue);
                        }
                        return;
                    }
                }
                throw new ScriptParsingException(el.PositionPair.File, el.PositionPair, EcmaScriptErrorKind.UnknownLabelTarget, el.Identifier);
            }
        }

        void WriteIfStatement(IfStatement el)
        {
            if (el.False == null)
            {
                WriteExpression(el.ExpressionElement);
                CallGetValue(el.ExpressionElement.Type);
                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
                var lFalse = fILG.DefineLabel();
                fILG.Emit(OpCodes.Brfalse, lFalse);
                WriteStatement(el.True);
                fILG.MarkLabel(lFalse);
            }
            else if (el.True == null)
            {
                WriteExpression(el.ExpressionElement);
                CallGetValue(el.ExpressionElement.Type);
                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
                var lTrue = fILG.DefineLabel();
                fILG.Emit(OpCodes.Brtrue, lTrue);
                WriteStatement(el.False);
                fILG.MarkLabel(lTrue);
            }
            else
            {
                WriteExpression(el.ExpressionElement);
                CallGetValue(el.ExpressionElement.Type);
                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
                var lFalse = fILG.DefineLabel();
                fILG.Emit(OpCodes.Brfalse, lFalse);
                WriteStatement(el.True);
                var lExit = fILG.DefineLabel();
                fILG.Emit(OpCodes.Br, lExit);
                fILG.MarkLabel(lFalse);
                WriteStatement(el.False);
                fILG.MarkLabel(lExit);
            }
        }

        IEnumerable<SourceElement> RecursiveFindFuncAndVars(IEnumerable<SourceElement> aElements)
        {
            foreach (var el in aElements)
            {
                if (el == null) continue;
                switch (el.Type)
                {
                    case ElementType.FunctionDeclaration:
                        yield return el;
                        break;
                    case ElementType.BlockStatement:
                        foreach (var e in RecursiveFindFuncAndVars(((BlockStatement)el).Items.Cast<SourceElement>()))
                            yield return e;
                        break;
                    case ElementType.IfStatement:
                        {
                            var ie = (IfStatement)el;
                            foreach (var e in RecursiveFindFuncAndVars(new[] { ie.True, ie.False }))
                                yield return e;
                        }
                        break;
                    case ElementType.LabelledStatement:
                        {
                            foreach (var e in RecursiveFindFuncAndVars(new[] { ((LabelledStatement)el).Statement }))
                                yield return e;
                        }
                        break;
                    case ElementType.SwitchStatement:
                        {
                            var ie = ((SwitchStatement)el).Clauses.Where(e => e.Body != null).Cast<SourceElement>();
                            foreach (var e in RecursiveFindFuncAndVars(ie))
                                yield return e;
                        }
                        break;
                    case ElementType.ForStatement:
                        {
                            var e = (ForStatement)el;
                            if (e.Initializers != null)
                                foreach (var i in RecursiveFindFuncAndVars(e.Initializers.Cast<SourceElement>()))
                                    yield return i;

                            foreach (var i in RecursiveFindFuncAndVars(new[] { e.Initializer, e.Increment, e.Comparison, e.Body }))
                                yield return i;
                        }
                        break;
                    case ElementType.ForInStatement:
                        {
                            var e = (ForInStatement)el;
                            foreach (var i in RecursiveFindFuncAndVars(new SourceElement[] { e.Initializer, e.LeftExpression, e.ExpressionElement, e.Body }))
                                yield return i;
                        }
                        break;
                    case ElementType.TryStatement:
                        {
                            var e = (TryStatement)el;
                            foreach (var i in RecursiveFindFuncAndVars(new[] { e.Body, e.Finally, e.Catch?.Body }))
                                yield return i;
                        }
                        break;
                    case ElementType.VariableStatement:
                        yield return el;
                        break;
                    case ElementType.VariableDeclaration:
                        yield return new VariableStatement(el.PositionPair, (VariableDeclaration)el);
                        break;
                    case ElementType.WithStatement:
                        foreach (var i in RecursiveFindFuncAndVars(new[] { ((WithStatement)el).Body }))
                            yield return i;
                        break;
                    case ElementType.DoStatement:
                        foreach (var i in RecursiveFindFuncAndVars(new[] { ((DoStatement)el).Body }))
                            yield return i;
                        break;
                    case ElementType.WhileStatement:
                        foreach (var i in RecursiveFindFuncAndVars(new[] { ((WhileStatement)el).Body }))
                            yield return i;
                        break;
                } // case
            }
        }

        void WriteExpression(ExpressionElement expression)
        {
            //var self = this;
            var lExpressionStack = new Stack<ExecutionStep>(128);
            lExpressionStack.Push(new ExecutionStep(expression, 0));

            while (true)
            {
                if (lExpressionStack.Count == 0)
                    break;

                var lExecutionStep = lExpressionStack.Pop();
                var lExpression = lExecutionStep.Expression;
                switch (lExpression.Type)
                {
                    case ElementType.ThisExpression:
                        fILG.Emit(OpCodes.Ldarg_1); // this is arg nr 1
                        break;
                    case ElementType.NullExpression:
                        fILG.Emit(OpCodes.Ldnull);
                        break;
                    case ElementType.StringExpression:
                        fILG.Emit(OpCodes.Ldstr, ((StringExpression)lExpression).Value);
                        break;
                    case ElementType.BooleanExpression:
                        {
                            if (((BooleanExpression)lExpression).Value)
                                fILG.Emit(OpCodes.Ldc_I4_1);
                            else
                                fILG.Emit(OpCodes.Ldc_I4_0);
                            fILG.Emit(OpCodes.Box, typeof(Boolean));
                        }
                        break;
                    case ElementType.IntegerExpression:
                        {
                            if (((IntegerExpression)lExpression).Value < (Int64)Int32.MinValue || ((IntegerExpression)lExpression).Value > (Int64)Int32.MaxValue)
                            {
                                fILG.Emit(OpCodes.Ldc_R8, (double)((IntegerExpression)lExpression).Value);
                                fILG.Emit(OpCodes.Box, typeof(Double));
                            }
                            else
                            {
                                fILG.Emit(OpCodes.Ldc_I4, ((IntegerExpression)lExpression).Value);
                                fILG.Emit(OpCodes.Box, typeof(Int32));
                            }
                        }
                        break;
                    case ElementType.DecimalExpression:
                        {
                            fILG.Emit(OpCodes.Ldc_R8, ((DecimalExpression)lExpression).Value);
                            fILG.Emit(OpCodes.Box, typeof(Double));
                        }
                        break;
                    case ElementType.RegExExpression:
                        {
                            
                            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                            fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                            fILG.Emit(OpCodes.Ldstr, ((RegExExpression)lExpression).String);
                            fILG.Emit(OpCodes.Ldstr, ((RegExExpression)lExpression).Modifier);
                            fILG.Emit(OpCodes.Newobj, typeof(EcmaScriptRegexpObject).GetConstructorInfo(new[] { typeof(GlobalObject), typeof(String), typeof(String) }));
                        }
                        break;
                    case ElementType.UnaryExpression:
                        {
                            switch (((UnaryExpression)lExpression).Operator)
                            {
                                case UnaryOperator.BinaryNot:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        CallGetValue(((UnaryExpression)lExpression).Value.Type);
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Operators.Method_BitwiseNot);
                                    }
                                    break;
                                case UnaryOperator.BoolNot:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        CallGetValue(((UnaryExpression)lExpression).Value.Type);
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Operators.Method_LogicalNot);
                                    }
                                    break;
                                case UnaryOperator.Delete:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Reference.Method_Delete);
                                        fILG.Emit(OpCodes.Box, typeof(Boolean));
                                    }
                                    break;
                                case UnaryOperator.Minus:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        CallGetValue(((UnaryExpression)lExpression).Value.Type);
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Operators.Method_Minus);
                                    }
                                    break;
                                case UnaryOperator.Plus:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        CallGetValue(((UnaryExpression)lExpression).Value.Type);
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Operators.Method_Plus);
                                    }
                                    break;
                                case UnaryOperator.PostDecrement:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Operators.Method_PostDecrement);
                                    }
                                    break;
                                case UnaryOperator.PostIncrement:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Operators.Method_PostIncrement);
                                    }
                                    break;
                                case UnaryOperator.PreDecrement:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Operators.Method_PreDecrement);
                                    }
                                    break;
                                case UnaryOperator.PreIncrement:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Operators.Method_PreIncrement);
                                    }
                                    break;
                                case UnaryOperator.TypeOf:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Operators.Method_TypeOf);
                                    }
                                    break;
                                case UnaryOperator.Void:
                                    {
                                        if (lExecutionStep.Step == 0)
                                        {
                                            lExpressionStack.Push(lExecutionStep.NextStep());
                                            lExpressionStack.Push(new ExecutionStep(((UnaryExpression)lExpression).Value));
                                            continue;
                                        }

                                        // Step 2
                                        CallGetValue(((UnaryExpression)lExpression).Value.Type);
                                        fILG.Emit(OpCodes.Pop);
                                        fILG.Emit(OpCodes.Call, Undefined.Method_Instance);
                                    }
                                    break;
                                default:
                                    throw new EcmaScriptException(lExpression.PositionPair.File, lExpression.PositionPair, EcmaScriptErrorKind.EInternalError, "Unknown type: " + lExpression.Type);
                            } // case
                        }
                        break;

                    case ElementType.IdentifierExpression:
                        {
                            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                            fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_LexicalScope);
                            fILG.Emit(OpCodes.Ldstr, ((IdentifierExpression)lExpression).Identifier);
                            if (fUseStrict)
                                fILG.Emit(OpCodes.Ldc_I4_1);
                            else
                                fILG.Emit(OpCodes.Ldc_I4_0);
                            fILG.Emit(OpCodes.Call, EnvironmentRecord.Method_GetIdentifier);
                        }
                        break;
                    case ElementType.BinaryExpression:
                        {
                            switch (((BinaryExpression)lExpression).Operator)
                            {
                                case BinaryOperator.Assign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.Plus:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Add);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.PlusAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Add);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.Divide:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Divide);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.DivideAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Divide);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.Minus:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Subtract);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.MinusAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Subtract);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.Modulus:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Modulus);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.ModulusAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Modulus);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.Multiply:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Multiply);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.MultiplyAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Multiply);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.ShiftLeft:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_ShiftLeft);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.ShiftLeftAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_ShiftLeft);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;

                                case BinaryOperator.ShiftRightSigned:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_ShiftRight);
                                            }
                                            break;
                                    }
                                    break;

                                case BinaryOperator.ShiftRightSignedAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_ShiftRight);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;

                                case BinaryOperator.ShiftRightUnsigned:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_ShiftRightUnsigned);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.ShiftRightUnsignedAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_ShiftRightUnsigned);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.And:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_And);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.AndAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_And);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.Or:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Or);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.OrAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Or);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.Xor:
                                case BinaryOperator.DoubleXor:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Xor);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.XorAssign:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                fILG.Emit(OpCodes.Dup);
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Xor);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Reference.Method_SetValue);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.Equal:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_Equal);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.NotEqual:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_NotEqual);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.StrictEqual:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_StrictEqual);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.StrictNotEqual:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_StrictNotEqual);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.Less:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_LessThan);
                                            }

                                            break;
                                    }
                                    break;
                                case BinaryOperator.Greater:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }

                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_GreaterThan);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.LessOrEqual:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_LessThanOrEqual);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.GreaterOrEqual:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_GreaterThanOrEqual);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.InstanceOf:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_InstanceOf);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.In:
                                    switch (lExecutionStep.Step)
                                    {
                                        case 0:
                                            {
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).LeftSide));
                                                continue;
                                            }
                                        case 1:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                                lExpressionStack.Push(lExecutionStep.NextStep());
                                                lExpressionStack.Push(new ExecutionStep(((BinaryExpression)lExpression).RightSide));
                                                continue;
                                            }
                                        case 2:
                                            {
                                                CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                                fILG.Emit(OpCodes.Call, Operators.Method_In);
                                            }
                                            break;
                                    }
                                    break;
                                case BinaryOperator.DoubleAnd:
                                    {
                                        WriteExpression(((BinaryExpression)lExpression).LeftSide);
                                        CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                        fILG.Emit(OpCodes.Dup);
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
                                        var lGotIt = fILG.DefineLabel();
                                        fILG.Emit(OpCodes.Brfalse, lGotIt);
                                        fILG.Emit(OpCodes.Pop);
                                        WriteExpression(((BinaryExpression)lExpression).RightSide);
                                        CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                        fILG.MarkLabel(lGotIt);
                                    }
                                    break;
                                case BinaryOperator.DoubleOr:
                                    {
                                        WriteExpression(((BinaryExpression)lExpression).LeftSide);
                                        CallGetValue(((BinaryExpression)lExpression).LeftSide.Type);
                                        fILG.Emit(OpCodes.Dup);
                                        fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                        fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
                                        var lGotIt = fILG.DefineLabel();
                                        fILG.Emit(OpCodes.Brtrue, lGotIt);
                                        fILG.Emit(OpCodes.Pop);
                                        WriteExpression(((BinaryExpression)lExpression).RightSide);
                                        CallGetValue(((BinaryExpression)lExpression).RightSide.Type);
                                        fILG.MarkLabel(lGotIt);
                                    }
                                    break;
                                default:
                                    throw new EcmaScriptException(lExpression.PositionPair.File, lExpression.PositionPair, EcmaScriptErrorKind.EInternalError, "Unknown type: " + lExpression.Type);
                            } // case
                        }
                        break;
                    case ElementType.ConditionalExpression:
                        {
                            WriteExpression(((ConditionalExpression)lExpression).Condition);
                            CallGetValue(((ConditionalExpression)lExpression).Condition.Type);
                            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                            fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsBoolean);
                            var lFalse = fILG.DefineLabel();
                            var lExit = fILG.DefineLabel();
                            fILG.Emit(OpCodes.Brfalse, lFalse);
                            WriteExpression(((ConditionalExpression)lExpression).True);
                            CallGetValue(((ConditionalExpression)lExpression).True.Type);
                            fILG.Emit(OpCodes.Br, lExit);
                            fILG.MarkLabel(lFalse);
                            WriteExpression(((ConditionalExpression)lExpression).False);
                            CallGetValue(((ConditionalExpression)lExpression).False.Type);
                            fILG.MarkLabel(lExit);
                        }
                        break;
                    case ElementType.ArrayLiteralExpression:
                        {
                            var items = ((ArrayLiteralExpression)lExpression).Items;
                            fILG.Emit(OpCodes.Ldc_I4, items.Count);
                            fILG.Emit(OpCodes.Conv_Ovf_U4);
                            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                            fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                            fILG.Emit(OpCodes.Newobj, EcmaScriptArrayObject.ConstructorInfo);
                            for (int i = 0, l = items.Count-1; i <= l; i++)
                            {
                                var el = items[i];
                                // TODO: Fix parser to exclude last empty element in array
                                // chapter11\11.1\11.1.4\11.1.4-0.js
                                if (null==el && i == l)
                                {
                                    continue;
                                }

                                fILG.Emit(OpCodes.Dup);
                                if (el == null)
                                {
                                    fILG.Emit(OpCodes.Call, Undefined.Method_Instance);
                                }
                                else
                                {
                                    WriteExpression(el);
                                    CallGetValue(el.Type);
                                }
                                fILG.Emit(OpCodes.Call, EcmaScriptArrayObject.Method_AddValue);
                            }
                        }
                        break;
                    case ElementType.ObjectLiteralExpression:
                        {
                            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                            fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                            fILG.Emit(OpCodes.Newobj, EcmaScriptObject.ConstructorInfo);
                            foreach (var el in ((ObjectLiteralExpression)lExpression).Items)
                            {
                                if (el.Name.Type == ElementType.IdentifierExpression)
                                    fILG.Emit(OpCodes.Ldstr, ((IdentifierExpression)el.Name).Identifier);
                                else
                                    WriteExpression(el.Name);
                                fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                fILG.Emit(OpCodes.Call, Utilities.Method_GetObjAsString);
                                fILG.Emit(OpCodes.Ldc_I4, (int)el.Mode);
                                WriteExpression(el.Value);   // Unwrapping this would be a nightmare
                                CallGetValue(el.Value.Type);
                                fILG.Emit(OpCodes.Ldc_I4, fUseStrict ? 1 : 0);
                                fILG.Emit(OpCodes.Call, EcmaScriptObject.Method_ObjectLiteralSet);
                            }
                        }
                        break;
                    case ElementType.SubExpression:
                        {
                            if (lExecutionStep.Step == 0)
                            {
                                lExpressionStack.Push(lExecutionStep.NextStep());
                                lExpressionStack.Push(new ExecutionStep(((SubExpression)lExpression).Member));
                                continue;
                            }

                            CallGetValue(((SubExpression)lExpression).Member.Type);
                            fILG.Emit(OpCodes.Ldstr, ((SubExpression)lExpression).Identifier);
                            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                            fILG.Emit(OpCodes.Ldc_I4, fUseStrict ? 1 : 0);
                            fILG.Emit(OpCodes.Call, Reference.Method_Create);
                        }
                        break;
                    case ElementType.ArrayAccessExpression:
                        switch (lExecutionStep.Step)
                        {
                            case 0:
                                {
                                    lExpressionStack.Push(lExecutionStep.NextStep());
                                    lExpressionStack.Push(new ExecutionStep(((ArrayAccessExpression)lExpression).Member));
                                    continue;
                                }
                            case 1:
                                {
                                    CallGetValue(((ArrayAccessExpression)lExpression).Member.Type);
                                    lExpressionStack.Push(lExecutionStep.NextStep());
                                    lExpressionStack.Push(new ExecutionStep(((ArrayAccessExpression)lExpression).Parameter));
                                    continue;
                                }
                            case 2:
                                {
                                    CallGetValue(((ArrayAccessExpression)lExpression).Parameter.Type);
                                    fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                                    fILG.Emit(OpCodes.Ldc_I4, fUseStrict ? 3 : 2);
                                    fILG.Emit(OpCodes.Call, Reference.Method_Create);
                                }
                                break;
                        }
                        break;

                    case ElementType.NewExpression:
                        {
                            if (lExecutionStep.Step == 0)
                            {
                                lExpressionStack.Push(lExecutionStep.NextStep());
                                lExpressionStack.Push(new ExecutionStep(((NewExpression)lExpression).Member));
                                continue;
                            }

                            // Step 2
                            CallGetValue(((NewExpression)lExpression).Member.Type);
                            fILG.Emit(OpCodes.Isinst, typeof(EcmaScriptObject));
                            fILG.Emit(OpCodes.Dup);
                            var lIsObject = fILG.DefineLabel();
                            fILG.Emit(OpCodes.Brtrue, lIsObject);
                            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                            fILG.Emit(OpCodes.Call, ExecutionContext.Method_get_Global);
                            fILG.Emit(OpCodes.Ldc_I4, (int)NativeErrorType.TypeError);
                            fILG.Emit(OpCodes.Ldstr, "Cannot instantiate non-object value");
                            fILG.Emit(OpCodes.Call, GlobalObject.Method_RaiseNativeError);
                            fILG.MarkLabel(lIsObject);
                            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                            fILG.Emit(OpCodes.Ldc_I4, ((NewExpression)lExpression).Parameters.Count);
                            fILG.Emit(OpCodes.Newarr, typeof(Object));
                            var n = 0;
                            foreach (var el in ((NewExpression)lExpression).Parameters)
                            {
                                fILG.Emit(OpCodes.Dup);
                                fILG.Emit(OpCodes.Ldc_I4, n);
                                WriteExpression(el);  // Unwrapping this would be a nightmare
                                CallGetValue(el.Type);
                                fILG.Emit(OpCodes.Stelem_Ref);
                                n++;
                            }
                            fILG.Emit(OpCodes.Callvirt, EcmaScriptObject.Method_Construct);
                        }
                        break;
                    case ElementType.CallExpression:
                        {
                            if (lExecutionStep.Step == 0)
                            {
                                lExpressionStack.Push(lExecutionStep.NextStep());
                                lExpressionStack.Push(new ExecutionStep(((CallExpression)lExpression).Member));
                                continue;
                            }

                            // Step 2
                            fILG.Emit(OpCodes.Ldarg_1); // self
                            fILG.Emit(OpCodes.Ldc_I4, ((CallExpression)lExpression).Parameters.Count);
                            fILG.Emit(OpCodes.Newarr, typeof(Object));
                            var n = 0;
                            foreach (var el in ((CallExpression)lExpression).Parameters)
                            {
                                fILG.Emit(OpCodes.Dup);
                                fILG.Emit(OpCodes.Ldc_I4, n);
                                WriteExpression(el); // Unwrapping this would be a nightmare
                                CallGetValue(el.Type);
                                fILG.Emit(OpCodes.Stelem_Ref);
                                n++;
                            }
                            fILG.Emit(OpCodes.Ldloc, fExecutionContext);
                            fILG.Emit(OpCodes.Call, EcmaScriptObject.Method_CallHelper);
                        }
                        break;
                    case ElementType.CommaSeparatedExpression: // only for for
                        {
                            var e = (CommaSeparatedExpression)lExpression;
                            for (int i = 0, l = e.Parameters.Count; i < l; i++)
                            {
                                if (i != 0)
                                {
                                    fILG.Emit(OpCodes.Pop);
                                    WriteExpression(e.Parameters[i]); // Unwrapping this would be a nightmare
                                    CallGetValue(e.Parameters[i].Type);
                                }
                            }
                        }
                        break;
                    case ElementType.FunctionExpression:
                        WriteFunction(((FunctionExpression)lExpression).Function, false);
                        break;
                    default:
                        throw new EcmaScriptException(lExpression.PositionPair.File, lExpression.PositionPair, EcmaScriptErrorKind.EInternalError, "Unknown type: " + lExecutionStep.Expression.Type);
                } // case
            }
        }
    }
}