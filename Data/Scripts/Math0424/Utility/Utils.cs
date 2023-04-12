using AnimationEngine.Language;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace AnimationEngine.Utility
{
    internal static class Utils
    {
        private static MyLog log;
        private static MyStringId SQUARE = MyStringId.GetOrCompute("Square");

        static Utils()
        {
            log = new MyLog(true);
#if !DEBUG
            log.Init("AnimationEngine.log", new System.Text.StringBuilder("0.1A"));
#endif
        }

        public static float Angle(this Vector3 one, Vector3 two)
        {
            return (float)(Math.Acos((one.Dot(two) / Math.Sqrt(one.LengthSquared()) * two.LengthSquared())) * 57.2957795131);
        }

        public static Vector3 FromAnglesToVector(this Vector3 me)
        {
            //x = yaw
            //y = pitch
            //z = roll
            return new Vector3(Math.Cos(me.X) * Math.Cos(me.Y), Math.Sin(me.X) * Math.Cos(me.Y), Math.Sin(me.Y));
        }

        public static Matrix Scale(this Matrix matrix, Vector3 vector)
        {
            matrix.M11 *= vector.X;
            matrix.M12 *= vector.X;
            matrix.M13 *= vector.X;

            matrix.M21 *= vector.Y;
            matrix.M22 *= vector.Y;
            matrix.M23 *= vector.Y;

            matrix.M31 *= vector.Z;
            matrix.M32 *= vector.Z;
            matrix.M33 *= vector.Z;

            return matrix;
        }

        public static Vector3 ToVector3(this Token[] vector)
        {
            if (vector.Length != 3)
            {
                throw new Exception($"Unable to convert vector with {vector.Length} elements");
            }
            return new Vector3(
                vector[0].Type == TokenType.FLOAT ? (float)vector[0].Value : (int)vector[0].Value,
                vector[1].Type == TokenType.FLOAT ? (float)vector[1].Value : (int)vector[1].Value,
                vector[2].Type == TokenType.FLOAT ? (float)vector[2].Value : (int)vector[2].Value
            );
        }

        public static void MessagePlayer(object msg)
        {
            log.WriteLine($"AnimationEngine MessagePlayer: {msg ?? "null"}");
            MyAPIGateway.Utilities.ShowMessage($"AnimationEngine", $"{msg ?? "null"}");
        }

        public static void LogToFile(object msg)
        {
#if !DEBUG
            log.WriteLine($"AnimationEngine: {msg ?? "null"}");
#else
            Console.WriteLine($"AnimationEngine: {msg ?? "null"}");
#endif
        }

        public static void DrawDebugLine(Vector3D pos, Vector3D dir, int r, int g, int b)
        {
            Vector4 color = new Vector4(r / 255f, g / 255f, b / 255f, 1);
            MySimpleObjectDraw.DrawLine(pos, pos + dir * 10, SQUARE, ref color, 0.01f);
        }

        public static void ConnectDebugLine(Vector3D pos, Vector3D pos2, int r, int g, int b)
        {
            Vector4 color = new Vector4(r / 255f, g / 255f, b / 255f, 1);
            MySimpleObjectDraw.DrawLine(pos, pos2, SQUARE, ref color, 0.01f);
        }

        public static string GetLogPath()
        {
            return log?.GetFilePath();
        }

        public static void CloseLog()
        {
            log?.Close();
        }

    }
}
