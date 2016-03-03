using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ArrayAccessExpression : ExpressionElement
    {
        ExpressionElement fParameter;
        ExpressionElement fMember;

        public ArrayAccessExpression(PositionPair aPositionPair, ExpressionElement aMember, ExpressionElement aParameter)
            : base(aPositionPair)
        {
            fParameter = aParameter;
            fMember = aMember;
        }

        public ExpressionElement Member { get { return fMember; } }
        public ExpressionElement Parameter { get { return fParameter; } }

        public override ElementType Type
        {
            get { return ElementType.ArrayAccessExpression; }
        }
    }
}
