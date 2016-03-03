using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public struct PositionPair
    {
        public PositionPair(Position aStart, Position aEnd)
        {
            StartRow = aStart.Row;
            StartCol = aStart.Col;
            StartPos = aStart.Pos;
            EndRow = aEnd.Row;
            EndCol = aEnd.Col;
            EndPos = aEnd.Pos;
            File = aStart.Module;
        }

        public PositionPair(int aStartPos = 0, int aStartRow = 0, int aStartCol = 0, int aEndPos = 0, int aEndRow = 0, int aEndCol = 0, string aFile = null)
        {
            StartRow = aStartRow;
            StartCol = aStartCol;
            StartPos = aStartPos;
            EndRow = aEndRow;
            EndCol = aEndCol;
            EndPos = aEndPos;
            File = aFile;
        }

        public int StartRow { get; set; }
        public int StartCol { get; set; }
        public int StartPos { get; set; }
        public int EndRow { get; set; }
        public int EndCol { get; set; }
        public int EndPos { get; set; }
        public string File { get; set; }

        public Position Start
        {
            get
            {
                return new Position(StartPos, StartRow, StartCol, File);
            }
        }

        public Position End
        {
            get
            {
                return new Position(EndPos, EndRow, EndCol, File);
            }
        }

        public bool IsValid
        {
            get
            {
                return (StartRow > 0) && !String.IsNullOrEmpty(File);
            }
        }
    }
}