using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ProgramElement : LanguageElement, IList<SourceElement>
    {
        List<SourceElement> fItems;

        public ProgramElement(PositionPair aPositionPair, SourceElement aItem) :
            base(aPositionPair)
        {
            fItems = new List<SourceElement> { aItem };
        }
        public ProgramElement(PositionPair aPositionPair, params SourceElement[] aItems) :
            base(aPositionPair)
        {
            fItems = new List<SourceElement>(aItems);
        }
        public ProgramElement(PositionPair aPositionPair, IEnumerable<SourceElement> aItems) :
            base(aPositionPair)
        {
            fItems = new List<SourceElement>(aItems);
        }
        public ProgramElement(PositionPair aPositionPair, List<SourceElement> aItems) :
            base(aPositionPair)
        {
            fItems = aItems;
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

        public override ElementType Type
        {
            get
            {
                return ElementType.Program;
            }
        }

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
