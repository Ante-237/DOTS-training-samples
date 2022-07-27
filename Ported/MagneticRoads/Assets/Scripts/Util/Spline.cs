using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace Util
{
    public static class Spline
    {
        private const int SplineLengthResolution = 10;
        
        public struct RoadTerminator
        {
            public float3 Position;
            public float3 Normal;
            public float3 Tangent;
        }
        
        public static float3 EvaluatePosition(RoadTerminator start, RoadTerminator end, float t)
        {
            var anchor1 = start.Position + start.Tangent;
            var anchor2 = end.Position - end.Tangent;
            // F(t) = (1-t)^3 P0 + 3(1-t)^2t P1 + 3(1-t)t^2 P2 + t^3 P3
            return start.Position * (1 - t) * (1 - t) * (1 - t) 
                   + 3 * anchor1 * (1 - t) * (1 - t) * t 
                   + 3 * anchor2 * (1 - t) * t * t 
                   + end.Position * t * t * t;
        }
        
        public static float3 EvaluateTangent(RoadTerminator start, RoadTerminator end, float t)
        {
            var anchor1 = start.Position + start.Tangent;
            var anchor2 = end.Position - end.Tangent;
            // F'(t) = 3(1-t)^2 (P1 - P0) + 6(1-t)(P2 - P1) + 3t^2(P3-P2)
            var derivative = 3 * (1 - t) * (1 - t) * (anchor1 - start.Position)
                + 6 * (1 - t) * (anchor2 - anchor1)
                + 3 * t * t * (end.Position - anchor2);

            return math.normalize(derivative);
        }

        public static quaternion EvaluateRotation(RoadTerminator start, RoadTerminator end, float t)
        {
            var startQuart = quaternion.LookRotation(start.Tangent, start.Normal);
            var endQuart = quaternion.LookRotation(end.Tangent, end.Normal);
            return math.slerp(startQuart, endQuart, t);
        }

        public static float EvaluateLength(RoadTerminator start, RoadTerminator end)
        {
            // Approximate spline with {SplineLengthResolution} number of straight lines
            // Sum all lengths
            float length = 0;
            for (int i = 0; i < SplineLengthResolution; i++)
            {
                float t0 = (float) i / SplineLengthResolution;
                float t1 = (float)(i + 1) / SplineLengthResolution;
                float3 pos0 = EvaluatePosition(start, end, t0);
                float3 pos1 = EvaluatePosition(start, end, t1);
                length += math.distance(pos0, pos1);
            }
            return length;
        }
    }
}