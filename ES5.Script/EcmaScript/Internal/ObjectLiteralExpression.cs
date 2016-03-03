using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class ObjectLiteralExpression : ExpressionElement, IList<PropertyAssignment>
    {
        List<PropertyAssignment> fItems;

        public ObjectLiteralExpression(PositionPair aPositionPair, params PropertyAssignment[] aItems) :
            base(aPositionPair)
        {
            fItems = new List<PropertyAssignment>(aItems);
        }
        public ObjectLiteralExpression(PositionPair aPositionPair, IEnumerable<PropertyAssignment> aItems) :
            base(aPositionPair)
        {
            fItems = new List<PropertyAssignment>(aItems);
        }
        public ObjectLiteralExpression(PositionPair aPositionPair, List<PropertyAssignment> aItems) :
            base(aPositionPair)
        {
            fItems = aItems;
        }

        public PropertyAssignment this[int index]
        {
            get
            {
                return ((IList<PropertyAssignment>)fItems)[index];
            }

            set
            {
                ((IList<PropertyAssignment>)fItems)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<PropertyAssignment>)fItems).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<PropertyAssignment>)fItems).IsReadOnly;
            }
        }

        public List<PropertyAssignment> Items
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
                return ElementType.ObjectLiteralExpression;
            }
        }

        public void Add(PropertyAssignment item)
        {
            ((IList<PropertyAssignment>)fItems).Add(item);
        }

        public void Clear()
        {
            ((IList<PropertyAssignment>)fItems).Clear();
        }

        public bool Contains(PropertyAssignment item)
        {
            return ((IList<PropertyAssignment>)fItems).Contains(item);
        }

        public void CopyTo(PropertyAssignment[] array, int arrayIndex)
        {
            ((IList<PropertyAssignment>)fItems).CopyTo(array, arrayIndex);
        }

        public IEnumerator<PropertyAssignment> GetEnumerator()
        {
            return ((IList<PropertyAssignment>)fItems).GetEnumerator();
        }

        public int IndexOf(PropertyAssignment item)
        {
            return ((IList<PropertyAssignment>)fItems).IndexOf(item);
        }

        public void Insert(int index, PropertyAssignment item)
        {
            ((IList<PropertyAssignment>)fItems).Insert(index, item);
        }

        public bool Remove(PropertyAssignment item)
        {
            return ((IList<PropertyAssignment>)fItems).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<PropertyAssignment>)fItems).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<PropertyAssignment>)fItems).GetEnumerator();
        }
    }
}
