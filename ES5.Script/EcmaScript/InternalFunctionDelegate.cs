using ES5.Script.EcmaScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ES5.Script.EcmaScript
{
    // TODO: check of this delegate
  public delegate object InternalFunctionDelegate(ExecutionContext aScope, object aSelf, object[] args, EcmaScriptInternalFunctionObject aFunc = null);
}
