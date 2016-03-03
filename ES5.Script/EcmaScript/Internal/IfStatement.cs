using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class IfStatement : Statement
    {
        Statement fFalse;
        Statement fTrue;
        ExpressionElement fExpression;

        public IfStatement(PositionPair aPositionPair, ExpressionElement aExpression, Statement aTrue, Statement aFalse = null) : base(aPositionPair)
        {
            fExpression = aExpression;
            fTrue = aTrue;
            fFalse = aFalse;
        }

        public ExpressionElement ExpressionElement { get { return fExpression; } }
        public Statement True { get { return fTrue; } }
        public Statement False { get { return fFalse; } }
        public override ElementType Type { get { return ElementType.IfStatement; } }
    }
}
