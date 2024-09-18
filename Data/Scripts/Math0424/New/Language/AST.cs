using System;
using System.Collections.Generic;

namespace AnimationEngine.Data.Scripts.Math0424.New.Language
{
    delegate ASTNode GrammarRule(Lexer.LexerToken[] arr, ref int index);

    struct ASTNode
    {
        public Lexer.LexerToken Token;
        public List<ASTNode> Children;
        public Lexer.LexerTokenValue Type
        {
            get { return Token.Type; }
        }
        public object Value
        {
            get { return Token.RawValue; }
        }

        public ASTNode(Lexer.LexerTokenValue value)
        {
            Token = new Lexer.LexerToken(value, null, 0, 0);
            Children = new List<ASTNode>();
        }
        public ASTNode(Lexer.LexerToken Token)
        {
            this.Token = Token;
            Children = new List<ASTNode>();
        }
        public ASTNode(Lexer.LexerTokenValue value, object Value)
        {
            Token = new Lexer.LexerToken(value, Value, 0, 0);
            Children = new List<ASTNode>();
        }
    }

    internal class AST
    {
        Lexer.LexerToken[] arr;
        int index;
        ASTNode root;
        GrammarRule grammar;

        public AST(Lexer.LexerToken[] arr, GrammarRule grammar)
        {
            this.arr = arr;
            index = 0;
            root = grammar(arr, ref index);
        }

        public void PrintAST()
        {
            PrintASTTree(root);
        }

        public void PrintASTTree(ASTNode node)
        {
            if (node.Value is Array)
            {
                Array array = (Array)node.Value;
                Logging.Debug($"| {node.Type}");
                Logging.IncreaseIndent();
                Logging.IncreaseIndent();
                foreach (var x in array)
                    Logging.Debug($"-'{x ?? "null"}'");
                Logging.DecreaseIndent();
                Logging.DecreaseIndent();
            }
            else
            {
                Logging.Debug($"| {node.Type} '{node.Value ?? "null"}'");
            }
            Logging.IncreaseIndent();
            foreach(var x in node.Children)
                PrintASTTree(x);
            Logging.DecreaseIndent();
        }
    }
}
