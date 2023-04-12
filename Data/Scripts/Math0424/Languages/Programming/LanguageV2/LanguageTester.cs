using AnimationEngine.Core;
using AnimationEngine.Language;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AnimationEngine.LanguageV2
{
    internal static class LanguageTester
    {

#if DEBUG
        public static void Main(string[] args)
        {
            try
            {
                Compile("Parenting.bsl");
                Test("Fibonacci.bsl", "func_fib", new SVariableInt(10));
                Test("LogicStatements.bsl", "func_logic");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void Compile(string script)
        {
            var mod = new VRage.Game.MyObjectBuilder_Checkpoint.ModItem
            {
                FriendlyName = "Test mod",
                Name = "Test mod",
            };

            ScriptRunner runner;
            var gen = new ScriptGenerator(mod, Path.Combine(Assembly.GetExecutingAssembly().Location, "..\\Data", "Scripts", "Math0424", "ExampleTestScripts", script), out runner);
        }

        private static void Test(string script, string function, params SVariable[] args)
        {
            var mod = new VRage.Game.MyObjectBuilder_Checkpoint.ModItem
            {
                FriendlyName = "Test mod",
                Name = "Test mod",
            };

            ScriptRunner runner;
            var gen = new ScriptGenerator(mod, Path.Combine(Assembly.GetExecutingAssembly().Location, "..\\Data", "Scripts", "Math0424", "ExampleTestScripts", script), out runner);
            Console.WriteLine($"Running script {script}");
            var arg = args.ToList();
            //arg.Insert(0, new SVariableInt(0));
            runner.Clone().Execute(function, arg.ToArray());
        }
#endif

    }
}
