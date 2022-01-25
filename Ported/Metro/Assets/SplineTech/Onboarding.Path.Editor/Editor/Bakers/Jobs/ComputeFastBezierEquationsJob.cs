using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Onboarding.BezierPath
{
    [BurstCompile(FloatMode = FloatMode.Fast)]
    public struct ComputeFastBezierEquationsJob : IJobParallelFor
    {
        [ReadOnly] 
        public NativeArray<Vector3> controlPoints;
        
        // output

        public NativeArray<BezierBakingJob.FastBezierEquation> fastBezierComponents;

        public void Execute(int i)
        {
            int offsetIn = i * 3;

            Vector3 p0 = controlPoints[offsetIn + 0];
            Vector3 p1 = controlPoints[offsetIn + 1];
            Vector3 p2 = controlPoints[offsetIn + 2];
            Vector3 p3 = controlPoints[offsetIn + 3];

            // Develop the Bezier equation so interpolation is faster later on
            //   P0(1-t)� + 3P1t(1-t)� + 3P2t�(1-t) + P3t�
            // = P0(1-3t+3t�-t�) + 3P1t(t�-2t+1) + 3P2(t�-t�) + P3t�
            // = P0-3P0t+3P0t�-P0t� + 3P1t�-6P1t�+3P1t + 3P2t�-3P2t� + P3t�
            // = P0 + t(-3P0+3P1) + t�(3P0-6P1+3P2) + t�(-P0+3P1-3P2+P3)
            int offsetOut = i * 4;
            fastBezierComponents[i] = new BezierBakingJob.FastBezierEquation
            {
                polynomialA = p0,
                polynomialB = -3 * p0 + 3 * p1,
                polynomialC = 3 * p0 - 6 * p1 + 3 * p2,
                polynomialD = -p0 + 3 * p1 - 3 * p2 + p3
            };
        }
    }
}
