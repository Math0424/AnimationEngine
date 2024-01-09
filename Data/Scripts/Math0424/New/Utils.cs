using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AnimationEngine.Data.Scripts.Math0424.New
{
    internal class Utils
    {

        static int _indentLevel = 0;
        static bool _debug = true;

        public enum LoggingMode
        {
            Debug,
            Info,
            Warning,
            Error,
        }

        public static void IncreaseIndent()
        {
            _indentLevel++;
        }

        public static void DecreaseIndent()
        {
            _indentLevel = Math.Max(0, --_indentLevel);
        }

        public static void Log(LoggingMode mode, object data)
        {
            if (!_debug && mode == LoggingMode.Debug)
                return;

            string indent = "".PadRight(_indentLevel);
            string[] arr = (data ?? 0).ToString().Split('\n');
            string dateTime = $"[{DateTime.Now}] [{mode.ToString().ToUpper()}] ";
            
            string combined = "";
            for(int i = 0; i < arr.Length; i++)
                if (i == 0)
                    combined += indent + arr[i] + "\n";
                else
                    combined += indent.PadRight(dateTime.Length) + arr[i] + "\n";
            Console.WriteLine($"{dateTime}{combined.Substring(0, combined.Length - 1)}");
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
