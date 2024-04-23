using AnimationEngine.Data.Scripts.Math0424.New.Language;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AnimationEngine
{
    internal class AnimationEngineV3
    {

        public static void Main(string[] args)
        {
            string path = Path.Combine(Assembly.GetExecutingAssembly().Location, "..\\Data", "Scripts", "Math0424", "New", "Tests");
            Lexer.Init();
            foreach (var file in Directory.GetFiles(path))
            {
                Compiler compiler = new Compiler(file);
            }
        }

    }
}
