using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class RegExExpression : ExpressionElement
    {
        string fString;
        string fModifier;

        public RegExExpression(PositionPair aPositionPair, string aString, string aModifier) :
            base(aPositionPair)
        {
            fString = aString;
            fModifier = aModifier;
        }

        public string String { get { return fString; } }
        public string Modifier { get { return fModifier; } }

        public override ElementType Type
        {
            get
            {
                return ElementType.RegExExpression;
            }
        }
    }
}