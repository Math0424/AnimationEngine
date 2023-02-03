using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.Language.Libs
{
    internal abstract class ScriptLib
    {
        protected Dictionary<string, Func<SVariable[], SVariable>> _dir = new Dictionary<string, Func<SVariable[], SVariable>>();

        protected void AddMethod(string name, Func<SVariable[], SVariable?> func)
        {
            _dir[name] = func;
        }

        public SVariable Execute(string value, SVariable[] arr)
        {
            if (_dir.ContainsKey(value))
            {
                return _dir[value].Invoke(arr);
            }
            return null;
        }
    }
}
