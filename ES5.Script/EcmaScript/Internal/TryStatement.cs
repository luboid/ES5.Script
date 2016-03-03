using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class TryStatement : Statement
    {
        CatchBlock fCatch;
        Statement fFinally; //FinallyBlock???
        Statement fBody;

        public TryStatement(PositionPair aPositionPair, Statement aBody, CatchBlock aCatch = null, Statement aFinally = null)
            : base(aPositionPair)
        {
            fBody = aBody;
            fFinally = aFinally;
            fCatch = aCatch;
        }

        public Statement Body
        {
            get
            { return fBody; }
        }

        public Statement Finally
        {
            get
            { return fFinally; }
        }

        public CatchBlock Catch
        {
            get
            { return fCatch; }
        }

        public FinallyInfo FinallyData { get; set; }

        public override ElementType Type
        {
            get { return ElementType.TryStatement; }
        }
    }
}
