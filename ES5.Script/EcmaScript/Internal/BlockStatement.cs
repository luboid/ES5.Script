using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class BlockStatement : Statement, IList<SourceElement>
    {
        List<SourceElement> fItems;

        public BlockStatement(PositionPair aPositionPair, params SourceElement[] aStatements) :
                    base(aPositionPair)
        {
            fItems = new List<SourceElement>(aStatements);
        }

        public BlockStatement(PositionPair aPositionPair, List<SourceElement> aStatements) :
                    base(aPositionPair)
        {
            fItems = aStatements;
        }

        public SourceElement this[int index]
        {
            get
            {
                return ((IList<SourceElement>)fItems)[index];
            }

            set
            {
                ((IList<SourceElement>)fItems)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<SourceElement>)fItems).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<SourceElement>)fItems).IsReadOnly;
            }
        }

        public List<SourceElement> Items
        {
            get
            {
                return fItems;
            }
        }

        public override ElementType Type { get { return ElementType.BlockStatement; } }

        public void Add(SourceElement item)
        {
            ((IList<SourceElement>)fItems).Add(item);
        }

        public void Clear()
        {
            ((IList<SourceElement>)fItems).Clear();
        }

        public bool Contains(SourceElement item)
        {
            return ((IList<SourceElement>)fItems).Contains(item);
        }

        public void CopyTo(SourceElement[] array, int arrayIndex)
        {
            ((IList<SourceElement>)fItems).CopyTo(array, arrayIndex);
        }

        public IEnumerator<SourceElement> GetEnumerator()
        {
            return ((IList<SourceElement>)fItems).GetEnumerator();
        }

        public int IndexOf(SourceElement item)
        {
            return ((IList<SourceElement>)fItems).IndexOf(item);
        }

        public void Insert(int index, SourceElement item)
        {
            ((IList<SourceElement>)fItems).Insert(index, item);
        }

        public bool Remove(SourceElement item)
        {
            return ((IList<SourceElement>)fItems).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<SourceElement>)fItems).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<SourceElement>)fItems).GetEnumerator();
        }
    }
}