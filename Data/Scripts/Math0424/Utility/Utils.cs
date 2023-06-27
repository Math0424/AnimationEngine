using AnimationEngine.Language;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using Line = VRageMath.Line;

namespace AnimationEngine.Utility
{
    internal static class Utils
    {
        private static MyLog log;
        private static MyStringId SQUARE = MyStringId.GetOrCompute("Square");
        private static readonly double c = Math.PI / 180;

        static Utils()
        {
            log = new MyLog(true);
#if !DEBUG
            log.Init("AnimationEngine.log", new System.Text.StringBuilder("Beta"));
#endif
        }

        public static List<string> MyRaycastDetectors(this IMyCubeBlock block, Vector3 pos1, Vector3 pos2)
        {
            List<string> returned = new List<string>();

            IHitInfo hit;
            if (MyAPIGateway.Physics.CastRay(pos1, pos2, out hit, 30))
                pos2 = hit.Position + (Vector3.Normalize(pos2-pos1) * 0.25f);

            MatrixD matrix = block.PositionComp.WorldMatrixNormalizedInv;
            Line line = new Line(pos1, pos2);
            Dictionary<string, IMyModelDummy> dummies = new Dictionary<string, IMyModelDummy>();
            block.Model.GetDummies(dummies);
            foreach (var x in dummies)
            {
                if (x.Key.StartsWith("subpart_"))
                    continue;
                Matrix dumMatrix = x.Value.Matrix * block.PositionComp.WorldMatrixRef;
                MyOrientedBoundingBox box = new MyOrientedBoundingBox(ref dumMatrix);
                float? value = box.Intersects(ref line);
                if (value.HasValue)
                    returned.Add(x.Key);
            }
            return returned;
        }

        public static QuaternionD EulerToQuat(this Vector3 me)
        {
            double cr = Math.Cos(me.X * c * 0.5);
            double sr = Math.Sin(me.X * c * 0.5);
            double cp = Math.Cos(me.Y * c * 0.5);
            double sp = Math.Sin(me.Y * c * 0.5);
            double cy = Math.Cos(me.Z * c * 0.5);
            double sy = Math.Sin(me.Z * c * 0.5);

            return new QuaternionD(
                    sr * cp * cy - cr * sp * sy,
                    cr * sp * cy + sr * cp * sy,
                    cr * cp * sy - sr * sp * cy,
                    cr * cp * cy + sr * sp * sy
                );
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

        public static void NotifyPlayer(object msg)
        {
            log.WriteLine($"AnimationEngine MessagePlayer: {msg ?? "null"}");
            MyAPIGateway.Utilities.ShowNotification($"AE: {msg ?? "null"}", 16, "Red");
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
