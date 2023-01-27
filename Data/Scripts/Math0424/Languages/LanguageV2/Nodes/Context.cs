using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AnimationEngine.LanguageV2.Nodes
{
    internal class Context
    {
        //compile
        private static Dictionary<string, int> GlobalDictionary = new Dictionary<string, int>();
        private static Dictionary<string, int> VariableDictionary = new Dictionary<string, int>();
        private static int StackIndex = 0;
        public static bool RequireReturn = false;
        public static string ClassContext = "";

        //parsing
        private static List<Token[]> ContextArray = new List<Token[]>();
        private static List<List<string>> Variables = new List<List<string>>();
        private static ScriptCreator Creator;

        public static void ResetStack()
        {
            VariableDictionary.Clear();
            if (StackIndex != 0)
            {
                throw new Exception($"Error stack size not zero ({StackIndex})");
            }
            StackIndex = 0;
        }

        public static int GetStackSize()
        {
            return StackIndex;
        }

        public static int PopStackIndex()
        {
            for (int i = 0; i < VariableDictionary.Count; i++)
            {
                var x = VariableDictionary.ElementAt(i);
                VariableDictionary[x.Key] = x.Value - 1;
            }
            return StackIndex--;
        }

        public static int IncreaseStackIndex()
        {
            for(int i = 0; i < VariableDictionary.Count; i++)
            {
                var x = VariableDictionary.ElementAt(i);
                VariableDictionary[x.Key] = x.Value + 1;
            }
            return StackIndex++;
        }

        public static bool IsGlobalVariable(string name)
        {
            return GlobalDictionary.ContainsKey(name.ToLower());
        }

        public static int GetGlobalVariable(string name)
        {
            return GlobalDictionary[name.ToLower()];
        }

        public static void AddGlobalVariable(string name)
        {
            GlobalDictionary[name.ToLower()] = GlobalDictionary.Count;
        }


        public static bool IsCompileVariable(string name)
        {
            return VariableDictionary.Keys.Contains(name.ToLower());
        }

        public static void RemoveCompileVariable(string name)
        {
            VariableDictionary.Remove(name.ToLower());
        }

        public static void AddCompileVariable(string name)
        {
            VariableDictionary[name.ToLower()] = 0;
        }

        public static int GetCompileVariableIndex(string name)
        {
            if (VariableDictionary.ContainsKey(name.ToLower()))
            {
                return VariableDictionary[name.ToLower()];
            }
            return -1;
        }






        public static void SetScript(ScriptCreator creator)
        {
            Creator = creator;
        }

        public static ScriptCreator GetScript()
        {
            return Creator;
        }

        public static Token[] GetTokens()
        {
            return ContextArray[ContextArray.Count - 1];
        }

        public static void PopTopContext()
        {
            Variables.RemoveAt(Variables.Count - 1);
            ContextArray.RemoveAt(ContextArray.Count - 1);
        }

        public static void EnterNewContext(Token[] arr)
        {
            ContextArray.Add(arr);
            Variables.Add(new List<string>());
        }

        public static void RemoveTokenRange(int start, int end)
        {
            List<Token> tl = new List<Token>(GetTokens());
            tl.RemoveRange(start, end - start);
            ContextArray[ContextArray.Count - 1] = tl.ToArray();
        }

        public static void AddVariable(string name)
        {
            Variables[Variables.Count - 1].Add(name.ToLower());
        }

        public static bool IsVariable(string name)
        {
            foreach(var x in Variables)
            {
                if (x.Contains(name.ToLower()))
                    return true;
            }
            return false;
        }

        public static void RemoveVariable(string name)
        {
            Variables[Variables.Count - 1].Remove(name.ToLower());
        }




    }
}
