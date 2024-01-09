using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AnimationEngine.Data.Scripts.Math0424.New
{
    internal class Utils
    {

        static int indentLevel;

        public enum LoggingMode
        {
            Debug,
            Info,
            Warning,
            Error,
        }

        public static void IncreaseIndent()
        {
            indentLevel++;
        }

        public static void DecreaseIndent()
        {
            indentLevel = Math.Max(0, indentLevel--);
        }

        public static void Log(LoggingMode mode, object data)
        {
            string indent = "".PadRight(indentLevel);
            Console.WriteLine($"[{DateTime.Now}] [{mode.ToString().ToUpper()}] {indent}{data ?? 0}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(object data)
        {
            Log(LoggingMode.Info, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(object data)
        {
            Log(LoggingMode.Warning, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(object data)
        {
            Log(LoggingMode.Debug, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(object data)
        {
            Log(LoggingMode.Error, data);
        }

    }
}
