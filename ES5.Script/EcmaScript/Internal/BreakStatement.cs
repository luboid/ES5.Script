﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class BreakStatement : Statement
    {
        string fIdentifier;

        public BreakStatement(PositionPair aPositionPair, string aIdentifier = null)
            : base(aPositionPair)
        {
            fIdentifier = aIdentifier;
        }

        public string Identifier
        {
            get
            { return fIdentifier; }
        }

        public override ElementType Type
        {
            get { return ElementType.BreakStatement; }
        }
    }
}