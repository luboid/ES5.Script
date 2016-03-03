using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public abstract class Statement : SourceElement
    {
        public Statement(PositionPair aPositionPair) :
            base(aPositionPair)
        { }
    }
}