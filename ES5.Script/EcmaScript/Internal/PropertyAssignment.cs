using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class PropertyAssignment : ExpressionElement
    {
        FunctionDeclarationType fMode;
        ExpressionElement fValue;
        PropertyBaseExpression fName;

        public PropertyAssignment(PositionPair aPositionPair, FunctionDeclarationType aMode, PropertyBaseExpression aName, ExpressionElement aValue)
            : base(aPositionPair)
        {
            fMode = aMode;
            fValue = aValue;
            fName = aName;
        }

        public FunctionDeclarationType Mode
        {
            get
            {
                return fMode;
            }
        }

        public ExpressionElement Value
        {
            get
            {
                return fValue;
            }
        }

        public PropertyBaseExpression Name
        {
            get
            {
                return fName;
            }
        }

        public override ElementType Type
        {
            get
            {
                return ElementType.PropertyAssignment;
            }
        }
    }
}