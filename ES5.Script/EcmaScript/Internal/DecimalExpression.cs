using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class DecimalExpression : LiteralExpression
    {
        double fValue;

        public DecimalExpression(PositionPair aPositionPair, double aValue) : 
            base(aPositionPair) 
        {
            fValue = aValue;
        }

        public double Value
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
                return ElementType.DecimalExpression;
            }
        }
    }
}
