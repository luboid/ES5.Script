using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public struct Position
    {
        int fRow;
        int fCol;
        int fPos;
        string fModule;

        public Position(int aPos = 0, int aRow = 0, int aCol = 0, string aModule = null)
        {
            fRow = aRow;
            fCol = aCol;
            fPos = aPos;
            fModule = aModule;
        }

        public int Row
        {
            get { return fRow; }
            set { fRow = value; }
        }
        public int Col
        {
            get { return fCol; }
            set { fCol = value; }
        }

        public int Pos
        {
            get { return fPos; }
            set { fPos = value; }
        }

        public int Column
        {
            get { return Col; }
        }

        public int Line
        {
            get { return Row; }
        }

        public string Module
        {
            get { return fModule; }
            set { fModule = value; }
        }
    }
}