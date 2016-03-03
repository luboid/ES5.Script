using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class VariableDeclaration: SourceElement
    {
        ExpressionElement fInitializer;
        string fIdentifier;

        public VariableDeclaration(PositionPair aPositionPair, string aIdentifier, ExpressionElement aInitializer = null) :
            base(aPositionPair)
        {
            fIdentifier = aIdentifier;
            fInitializer = aInitializer;
        }

        public string Identifier
        {
            get
            { return fIdentifier; }
        }

        public ExpressionElement Initializer { get { return fInitializer; } }
        public override ElementType Type
        {
            get { return ElementType.VariableDeclaration; }
        }
    }
}
