using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class CatchBlock : SourceElement
    {
        Statement fBody;
        string fIdentifier;

        public CatchBlock(PositionPair aPositionPair, string anIdentifier, Statement aBody)
            : base(aPositionPair)
        {
            fIdentifier = anIdentifier;
            fBody = aBody;
        }

        public string Identifier
        {
            get
            { return fIdentifier; }
        }

        public Statement Body { get { return fBody; } }

        public override ElementType Type
        {
            get { return ElementType.CatchBlock; }
        }
    }
}