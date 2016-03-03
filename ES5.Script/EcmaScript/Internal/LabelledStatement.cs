using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class LabelledStatement : IterationStatement
    {
        Statement fStatement;
        string fIdentifier;

        public LabelledStatement(PositionPair aPositionPair, string anIdentifier, Statement aStatement)
            : base(aPositionPair)
        {
            fIdentifier = anIdentifier;
            fStatement = aStatement;
        }

        public string Identifier
        {
            get { return fIdentifier; }
        }

        public Statement Statement
        {
            get
            { return fStatement; }
        }

        public override ElementType Type
        {
            get { return ElementType.LabelledStatement; }
        }
    }
}
