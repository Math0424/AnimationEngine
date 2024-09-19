using AnimationEngine.Data.Scripts.Math0424.New.Language;
using System.IO;
using System.Reflection;

namespace AnimationEngine
{
    internal class AnimationEngineV3
    {

#if DEBUG
        public static void Main(string[] args)
        {
            string path = Path.Combine(Assembly.GetExecutingAssembly().Location, "..\\Data", "Scripts", "Math0424", "New", "Tests");
            Lexer.Init();
            foreach (var file in Directory.GetFiles(path))
            {
                Compiler.Compile(file);
            }
        }
#endif

    }
}
