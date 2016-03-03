using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class FunctionDeclarationElement : SourceElement, IList<SourceElement>
    {
        FunctionDeclarationType fMode;
        string fIdentifier;
        List<ParameterDeclaration> fParameters;
        List<SourceElement> fItems;

        public FunctionDeclarationElement(PositionPair aPositionPair, FunctionDeclarationType aMode, string anIdentifier, ParameterDeclaration[] aParameters, SourceElement[] aItems) :
            base(aPositionPair)
        {
            fIdentifier = anIdentifier;
            fParameters = new List<ParameterDeclaration>(aParameters);
            fItems = new List<SourceElement>(aItems);
            fMode = aMode;
        }

        public FunctionDeclarationElement(PositionPair aPositionPair, FunctionDeclarationType aMode, string anIdentifier, List<ParameterDeclaration> aParameters, List<SourceElement> aItems) :
                    base(aPositionPair)
        {
            fIdentifier = anIdentifier;
            fParameters = aParameters;
            fItems = aItems;
            fMode = aMode;
        }

        public List<SourceElement> Items { get { return fItems; } }
        public string Identifier { get { return fIdentifier;} }
        public List<ParameterDeclaration> Parameters { get { return fParameters; } }
        public FunctionDeclarationType Mode { get { return fMode; } }
        public override ElementType Type
        {
            get { return ElementType.FunctionDeclaration; }
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

        public int IndexOf(SourceElement item)
        {
            return ((IList<SourceElement>)fItems).IndexOf(item);
        }

        public void Insert(int index, SourceElement item)
        {
            ((IList<SourceElement>)fItems).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<SourceElement>)fItems).RemoveAt(index);
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

        public bool Remove(SourceElement item)
        {
            return ((IList<SourceElement>)fItems).Remove(item);
        }

        public IEnumerator<SourceElement> GetEnumerator()
        {
            return ((IList<SourceElement>)fItems).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<SourceElement>)fItems).GetEnumerator();
        }
    }
}
