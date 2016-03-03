using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class IdentifierExpression : PropertyBaseExpression
    {
        string fIdentifier;

        public IdentifierExpression(PositionPair aPositionPair, string aIdentifier)
            : base(aPositionPair)
        {
            fIdentifier = aIdentifier;
        }

        public string Identifier { get { return fIdentifier; } }
        public override object ObjectValue { get { return fIdentifier; } }
        public override ElementType Type { get { return ElementType.IdentifierExpression; } }
    }
}