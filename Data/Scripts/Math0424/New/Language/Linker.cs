using System;
using System.IO;

namespace AnimationEngine.Data.Scripts.Math0424.New.Language
{
    internal class Linker
    {
        public static void LinkASTFiles(string path, AST ast)
        {
            for (int i = 0; i < ast.Root.Children.Count; i++)
            {
                var x = ast.Root.Children[i];
                if (x.Type == Lexer.LexerTokenValue.IMPORT)
                {
                    string filePath = Path.Combine(Path.GetDirectoryName(path), (string)x.Token.RawValue);
                    if (!File.Exists(filePath))
                        throw new Exception($"Cannot find linking file '{filePath}'");
                    
                    var ast2 = Compiler.GenerateAST(filePath);
                    ast.Root.Children.RemoveAt(i);
                    foreach (var y in ast2.Root.Children)
                        ast.Root.Children.Insert(i, y);
                }
            }
        }
    }
}
