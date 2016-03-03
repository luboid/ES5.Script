using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ConditionalExpression : ExpressionElement
    {
        ExpressionElement fCondition;
        ExpressionElement fTrue;
        ExpressionElement fFalse;

        public ConditionalExpression(PositionPair aPositionPair, ExpressionElement aCondition, ExpressionElement aTrue, ExpressionElement aFalse)
            : base(aPositionPair)
        {
            fCondition = aCondition;
            fTrue = aTrue;
            fFalse = aFalse;
        }

        public ExpressionElement Condition { get { return fCondition; } }
        public ExpressionElement True { get { return fTrue; } }
        public ExpressionElement False { get { return fFalse; } }
        public override ElementType Type { get { return ElementType.ConditionalExpression; } }

    }
}
