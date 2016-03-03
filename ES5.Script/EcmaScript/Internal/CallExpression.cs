using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class CallExpression : ExpressionElement
    {
        List<ExpressionElement> fParameters;
        ExpressionElement fMember;

        public CallExpression(PositionPair aPositionPair, ExpressionElement aMember, params ExpressionElement[] aParameters)
            : base(aPositionPair)
        {
            fMember = aMember;
            fParameters = new List<ExpressionElement>(aParameters);
        }

        public CallExpression(PositionPair aPositionPair, ExpressionElement aMember, IEnumerable<ExpressionElement> aParameters)
            : base(aPositionPair)
        {
            fMember = aMember;
            fParameters = new List<ExpressionElement>(aParameters);
        }

        public CallExpression(PositionPair aPositionPair, ExpressionElement aMember, List<ExpressionElement> aParameters)
            : base(aPositionPair)
        {
            fMember = aMember;
            fParameters = aParameters;
        }

        public ExpressionElement Member { get { return fMember; } }
        public List<ExpressionElement> Parameters { get { return fParameters; } }
        public override ElementType Type { get { return ElementType.CallExpression; } }
    }
}
