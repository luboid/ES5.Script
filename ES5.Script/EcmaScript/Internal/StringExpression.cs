using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class StringExpression : LiteralExpression
    {
        string fValue;

        public StringExpression(PositionPair aPositionPair, string aValue) : 
            base(aPositionPair) 
        {
            fValue = aValue;
        }

        public string Value
        {
            get
            {
                return fValue;
            }
        }

        public override object ObjectValue
        {
            get
            {
                return fValue;
            }
        }

        public override ElementType Type
        {
            get
            {
                return ElementType.StringExpression;
            }
        }
    }
}
