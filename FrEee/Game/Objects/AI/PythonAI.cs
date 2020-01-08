﻿using FrEee.Modding;
using FrEee.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrEee.Game.Objects.AI
{
    [Serializable]
    public class PythonAI<TDomain, TContext> : AI<TDomain, TContext>
    {
        public PythonAI(string name, PythonScript script, SafeDictionary<string, ICollection<string>> ministerNames) : base(name, script, ministerNames)
        {
        }

        public override void Act(TDomain domain, TContext context)
        {
       
            var variables = new Dictionary<string, object>();
            variables.Add("domain", domain);
            var readOnlyVariables = new Dictionary<string, object>();
            readOnlyVariables.Add("context", context);
            readOnlyVariables.Add("enabledMinisters", EnabledMinisters);
            PythonScriptEngine.RunScript<object>(Script as PythonScript, variables, readOnlyVariables);
            
        }
    }
}
