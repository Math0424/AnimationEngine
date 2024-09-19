using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnimationEngine.Data.Scripts.Math0424.New.Language
{
    // need to be a class?
    internal class Compiler
    {

#if DEBUG
        public static void Compile(string path)
        {
            string[] rawFile = File.ReadAllLines(path);
            try
            {
                long start = DateTime.Now.Ticks;
                
                var ast = GenerateAST(path);
                Linker.LinkASTFiles(path, ast);

                Logging.Info($"Compiled script ({(DateTime.Now.Ticks - start) / TimeSpan.TicksPerMillisecond}ms)");

                ast.PrintAST();
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
        }

        public static AST GenerateAST(string path)
        {
            var lines = ReadFile(path);
            var tokens = Lexer.Parse(Path.GetFileName(path), lines);
            return GenerateAST(tokens);
        }

        private static string[] ReadFile(string path)
        {
            if (!File.Exists(path))
            {
                Logging.Error($"Cannot find file at '{path}'");
                return null;
            }
            return File.ReadAllLines(path);
        }

        private static AST GenerateAST(Lexer.LexerToken[] tokens)
        {
            var headers = ReadHeaders(ref tokens);
            if (!headers.ContainsKey("version") || headers["version"].GetType() != typeof(int))
                throw new Exception($"Missing version header!");
            AST ast;
            switch (((int)headers["version"]))
            {
                case 1:
                    ast = null;
                    break;
                case 2:
                    ast = null;
                    break;
                case 3:
                    ast = new AST(tokens, RulesLanguageV3.Program);
                    break;
                default:
                    throw new Exception($"Cannot read scripts with version {headers["version"]}");
            }
            return ast;
        }
#endif

        public static Dictionary<string, object> ReadHeaders(ref Lexer.LexerToken[] arr)
        {
            Dictionary<string, object> headers = new Dictionary<string, object>();

            int index = 0;
            while(index < arr.Length)
            {
                if (arr[index].Type == Lexer.LexerTokenValue.AT)
                {
                    if (index + 2 > arr.Length || arr[index + 1].Type != Lexer.LexerTokenValue.KEYWORD)
                        throw new Exception($"Invalid header on line {arr[index].LineNumber + 1}");
                    string headerName = arr[index + 1].RawValue.ToString().ToLower();
                    if (headers.ContainsKey(headerName))
                        throw new Exception($"Duplicate header on line {arr[index].LineNumber + 1}");
                    headers[headerName] = arr[index + 2].RawValue;
                }
                index++;
            }

            return headers;
        }

    }
}
