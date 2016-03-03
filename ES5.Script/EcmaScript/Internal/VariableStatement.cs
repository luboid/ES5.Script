using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript.Internal
{
    public class VariableStatement : Statement, IList<VariableDeclaration>
    {
        List<VariableDeclaration> fVariables;

        public VariableStatement(PositionPair aPositionPair, params VariableDeclaration[] aVariables) :
            base(aPositionPair)
        {
            fVariables = new List<VariableDeclaration>(aVariables);
        }
        public VariableStatement(PositionPair aPositionPair, List<VariableDeclaration> aVariables) :
            base(aPositionPair)
        {
            fVariables = aVariables;
        }

        public VariableDeclaration this[int index]
        {
            get
            {
                return ((IList<VariableDeclaration>)fVariables)[index];
            }

            set
            {
                ((IList<VariableDeclaration>)fVariables)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<VariableDeclaration>)fVariables).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<VariableDeclaration>)fVariables).IsReadOnly;
            }
        }

        public List<VariableDeclaration> Items { get { return fVariables; } }
        public override ElementType Type { get { return ElementType.VariableStatement; } }

        public void Add(VariableDeclaration item)
        {
            ((IList<VariableDeclaration>)fVariables).Add(item);
        }

        public void Clear()
        {
            ((IList<VariableDeclaration>)fVariables).Clear();
        }

        public bool Contains(VariableDeclaration item)
        {
            return ((IList<VariableDeclaration>)fVariables).Contains(item);
        }

        public void CopyTo(VariableDeclaration[] array, int arrayIndex)
        {
            ((IList<VariableDeclaration>)fVariables).CopyTo(array, arrayIndex);
        }

        public IEnumerator<VariableDeclaration> GetEnumerator()
        {
            return ((IList<VariableDeclaration>)fVariables).GetEnumerator();
        }

        public int IndexOf(VariableDeclaration item)
        {
            return ((IList<VariableDeclaration>)fVariables).IndexOf(item);
        }

        public void Insert(int index, VariableDeclaration item)
        {
            ((IList<VariableDeclaration>)fVariables).Insert(index, item);
        }

        public bool Remove(VariableDeclaration item)
        {
            return ((IList<VariableDeclaration>)fVariables).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<VariableDeclaration>)fVariables).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<VariableDeclaration>)fVariables).GetEnumerator();
        }
    }
}
