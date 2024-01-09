using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnimationEngine.Data.Scripts.Math0424.New.Language
{
    // need to be a class?
    internal class Compiler
    {

        public Compiler(string path)
        {
            if (!File.Exists(path))
            {
                Utils.Error($"Cannot find file at '{path}'");
                return;
            }
            string[] rawFile = File.ReadAllLines(path);
            try
            {
                Lexer.LexerToken[] lexerScript = Lexer.Parse(rawFile);
                foreach(var x in lexerScript)
                    Utils.Debug(x);
            } 
            catch (Exception ex)
            {
                Utils.Error(ex.Message);
            }
        }

    }
}
