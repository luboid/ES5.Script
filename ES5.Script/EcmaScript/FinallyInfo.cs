using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;


namespace ES5.Script.EcmaScript
{
    public class FinallyInfo
    {
        List<Label> jumpTable = new List<Label>();

        public Label FinallyLabel { get; set; }
        public LocalBuilder FinallyState { get; set; }
        public List<Label> JumpTable { get { return jumpTable; } }

        public int AddUnique(Label aLabel)
        {
            for (int i = 0, l = JumpTable.Count; i < l; i++)
                if (JumpTable[i].Equals(aLabel)) return i;

            JumpTable.Add(aLabel);
            return JumpTable.Count - 1;
        }
    }
}