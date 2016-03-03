using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ReturnStatement : Statement
    {
        ExpressionElement fExpression;

        public ReturnStatement(PositionPair aPositionPair, ExpressionElement aExpression = null)
                : base(aPositionPair)
        {
            fExpression = aExpression;
        }

        public ExpressionElement ExpressionElement { get {  return fExpression;} }
        public override ElementType Type { get { return ElementType.ReturnStatement; } }
    }
}
