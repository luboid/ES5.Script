using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class SwitchStatement : IterationStatement, IList<CaseClause>
    {
        List<CaseClause> fClauses;
        ExpressionElement fExpression;

        public SwitchStatement(PositionPair aPositionPair, ExpressionElement anExpression, params CaseClause[] aClauses)
            : base(aPositionPair)
        {
            fExpression = anExpression;
            fClauses = new List<CaseClause>(aClauses);
        }

        public SwitchStatement(PositionPair aPositionPair, ExpressionElement anExpression, IEnumerable<CaseClause> aClauses)
            : base(aPositionPair)
        {
            fExpression = anExpression;
            fClauses = new List<CaseClause>(aClauses);
        }

        public SwitchStatement(PositionPair aPositionPair, ExpressionElement anExpression, List<CaseClause> aClauses)
            : base(aPositionPair)
        {
            fExpression = anExpression;
            fClauses = aClauses;
        }

        public ExpressionElement ExpressionElement
        {
            get
            { return fExpression; }
        }

        public List<CaseClause> Clauses
        {
            get
            { return fClauses; }
        }

        public override ElementType Type
        {
            get { return ElementType.SwitchStatement; }
        }

        public int Count
        {
            get
            {
                return ((IList<CaseClause>)fClauses).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<CaseClause>)fClauses).IsReadOnly;
            }
        }

        public CaseClause this[int index]
        {
            get
            {
                return ((IList<CaseClause>)fClauses)[index];
            }

            set
            {
                ((IList<CaseClause>)fClauses)[index] = value;
            }
        }

        public int IndexOf(CaseClause item)
        {
            return ((IList<CaseClause>)fClauses).IndexOf(item);
        }

        public void Insert(int index, CaseClause item)
        {
            ((IList<CaseClause>)fClauses).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<CaseClause>)fClauses).RemoveAt(index);
        }

        public void Add(CaseClause item)
        {
            ((IList<CaseClause>)fClauses).Add(item);
        }

        public void Clear()
        {
            ((IList<CaseClause>)fClauses).Clear();
        }

        public bool Contains(CaseClause item)
        {
            return ((IList<CaseClause>)fClauses).Contains(item);
        }

        public void CopyTo(CaseClause[] array, int arrayIndex)
        {
            ((IList<CaseClause>)fClauses).CopyTo(array, arrayIndex);
        }

        public bool Remove(CaseClause item)
        {
            return ((IList<CaseClause>)fClauses).Remove(item);
        }

        public IEnumerator<CaseClause> GetEnumerator()
        {
            return ((IList<CaseClause>)fClauses).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<CaseClause>)fClauses).GetEnumerator();
        }
    }
}
