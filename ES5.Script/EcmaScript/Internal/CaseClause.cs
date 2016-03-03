using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class CaseClause : SourceElement, IList<Statement>
    {
        List<Statement> fBody;
        ExpressionElement fExpression;

        public CaseClause(PositionPair aPositionPair, ExpressionElement anExpression, params Statement[] aBody)
            : base(aPositionPair)
        {
            fExpression = anExpression;
            fBody = new List<Statement>(aBody);
        }

        public CaseClause(PositionPair aPositionPair, ExpressionElement anExpression, IEnumerable<Statement> aBody)
            : base(aPositionPair)
        {
            fExpression = anExpression;
            fBody = new List<Statement>(aBody);
        }

        public CaseClause(PositionPair aPositionPair, ExpressionElement anExpression, List<Statement> aBody)
            : base(aPositionPair)
        {
            fExpression = anExpression;
            fBody = aBody;
        }

        public ExpressionElement ExpressionElement { get { return fExpression; } }
        public List<Statement> Body { get { return fBody; } }
        public bool IsDefault { get { return fExpression == null; } }
        public override ElementType Type { get { return ElementType.CaseClause; } }

        public int Count
        {
            get
            {
                return ((IList<Statement>)fBody).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<Statement>)fBody).IsReadOnly;
            }
        }

        public Statement this[int index]
        {
            get
            {
                return ((IList<Statement>)fBody)[index];
            }

            set
            {
                ((IList<Statement>)fBody)[index] = value;
            }
        }

        public int IndexOf(Statement item)
        {
            return ((IList<Statement>)fBody).IndexOf(item);
        }

        public void Insert(int index, Statement item)
        {
            ((IList<Statement>)fBody).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<Statement>)fBody).RemoveAt(index);
        }

        public void Add(Statement item)
        {
            ((IList<Statement>)fBody).Add(item);
        }

        public void Clear()
        {
            ((IList<Statement>)fBody).Clear();
        }

        public bool Contains(Statement item)
        {
            return ((IList<Statement>)fBody).Contains(item);
        }

        public void CopyTo(Statement[] array, int arrayIndex)
        {
            ((IList<Statement>)fBody).CopyTo(array, arrayIndex);
        }

        public bool Remove(Statement item)
        {
            return ((IList<Statement>)fBody).Remove(item);
        }

        public IEnumerator<Statement> GetEnumerator()
        {
            return ((IList<Statement>)fBody).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Statement>)fBody).GetEnumerator();
        }
    }
}
