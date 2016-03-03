using ES5.Script.EcmaScript;
using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script
{
    public class ScriptScope : DeclarativeEnvironmentRecord
    {
        public ScriptScope(EnvironmentRecord aPrevious, GlobalObject aGlobal)
            : base(aPrevious, aGlobal)
        { }

        public bool ContainsVariable(string name)
        {
            return Bag.ContainsKey(name);
        }

        public IEnumerable<KeyValuePair<String, Object>> GetItems()
        {
            foreach (var el in Bag)
                yield return new KeyValuePair<String, Object>(el.Key, el.Value.Value != null ? el.Value?.Value : Undefined.Instance);
        }

        public T GetVariable<T>(string name)
            where T : class
        {
            return GetVariable(name) as T;
        }

        public object GetVariable(string name)
        {
            var lWork = Bag[name];

            return lWork != null ? lWork.Value : Undefined.Instance;
        }

        public IEnumerable<String> GetVariableNames()
        {
            return Bag.Keys;
        }

        public bool RemoveVariable(string name)
        {
            return base.DeleteBinding(name);
        }

        public void SetVariable(string name, object value)
        {
            if (!Bag.ContainsKey(name)) CreateMutableBinding(name, true);
            SetMutableBinding(name, value, true);
        }

        public bool TryGetVariable(string name, out object value)
        {
            var result = Bag.ContainsKey(name);
            if (result) value = GetVariable(name); else value = null;
            return result;
        }

        public bool TryGetVariable<T>(string name, out T value)
            where T : class
        {
            var result = Bag.ContainsKey(name);
            if (result) value = GetVariable<T>(name); else value = default(T);
            return result;
        }

        public override void SetMutableBinding(string aName, object aValue, bool aStrict)
        {
            base.SetMutableBinding(aName, TryWrap(aValue), aStrict);
        }

        public virtual object TryWrap(object aValue)
        {
            return aValue;
        }
    }
}
