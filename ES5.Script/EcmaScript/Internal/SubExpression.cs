using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class SubExpression : ExpressionElement
    {
        String fIdentifier;
        ExpressionElement fMember;

        public SubExpression(PositionPair aPositionPair, ExpressionElement aMember, string anIdentifier)
            : base (aPositionPair)
        {
            fIdentifier = anIdentifier;
            fMember = aMember;
        }

        public ExpressionElement Member { get { return fMember; } }
        public string Identifier { get { return fIdentifier; } }
        public override ElementType Type { get { return ElementType.SubExpression; } }
    }
}
