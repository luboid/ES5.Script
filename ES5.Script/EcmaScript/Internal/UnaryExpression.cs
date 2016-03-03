using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class UnaryExpression : ExpressionElement
    {
        UnaryOperator fOperator;
        ExpressionElement fValue;

        public UnaryExpression(PositionPair aPositionPair, ExpressionElement aValue, UnaryOperator anOperator)
            : base(aPositionPair)
        {
            fValue = aValue;
            fOperator = anOperator;
        }

        public ExpressionElement Value
        {
            get
            { return fValue; }
        }
        public UnaryOperator Operator
        {
            get
            { return fOperator; }
        }

        public override ElementType Type
        {
            get { return ElementType.UnaryExpression; }
        }

    }
}
