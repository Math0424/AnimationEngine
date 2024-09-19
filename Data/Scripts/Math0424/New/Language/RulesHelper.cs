using System;
using System.Collections.Generic;
using System.Text;

namespace AnimationEngine.Data.Scripts.Math0424.New.Language
{
    internal class RulesHelper
    {
        protected static bool ExpectNext(Lexer.LexerToken[] arr, int index, Lexer.LexerTokenValue value)
        {
            if (index + 1 >= arr.Length)
                throw new Exception("Trying to access Token outside of Token range", new Exception($"Trying to access index {index + 1} and check for {value}"));
            return arr[index + 1].Type == value;
        }

        protected static void EndLine(Lexer.LexerToken[] arr, ref int index)
        {
            while (index < arr.Length && arr[index].Type != Lexer.LexerTokenValue.SEMICOLON)
                index++;
        }

        protected static bool Next(Lexer.LexerToken[] arr, ref int index, Lexer.LexerTokenValue value)
        {
            while (index < arr.Length && arr[index].Type != value)
                index++;
            return index != arr.Length;
        }

        protected static bool StrictNext(Lexer.LexerToken[] arr, ref int index, Lexer.LexerTokenValue value)
        {
            while (index < arr.Length)
            {
                if (arr[index].Type == value)
                    return true;
                else if (arr[index].Type != Lexer.LexerTokenValue.SEMICOLON)
                    return false;
                index++;
            }
            return false;
        }

        protected static bool HasNext(Lexer.LexerToken[] arr, int index, params Lexer.LexerTokenValue[] values)
        {
            while (index < arr.Length && !values.Contains(arr[index].Type))
                index++;
            return index != arr.Length;
        }

        protected static Lexer.LexerToken[] ValuesBetween(int a, int b, Lexer.LexerToken[] arr)
        {
            Lexer.LexerToken[] ret = new Lexer.LexerToken[a - b];
            Array.Copy(arr, ret, a - b);
            return ret;
        }
    }
}
