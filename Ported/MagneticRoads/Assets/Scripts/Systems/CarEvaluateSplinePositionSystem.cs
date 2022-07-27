using Aspects;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Util;

namespace Systems
{
    [WithAll(typeof(Car))]
    [BurstCompile]
    public partial struct CarEvaluateSplinePositionJob : IJobEntity
    {
        [ReadOnly]
        public ComponentDataFromEntity<RoadSegment> RoadSegmentFromEntity;
        public float DT;

        void Execute(ref CarAspect carAspect)
        {
            RoadSegmentFromEntity.TryGetComponent(carAspect.RoadSegment, out RoadSegment rs);
            var carT = math.clamp(carAspect.T + ((carAspect.Speed * DT / rs.Length)), 0, 1);
            
            // Cars in lane 2 and 4 go backwards alone the spline
            var directionalT = carAspect.LaneNumber % 2 == 1 ? carT : -carT;
            
            // Center of the spline
            var splinePos = Spline.EvaluatePosition(rs.Start, rs.End, directionalT);

            // TODO: local2world
            var offset = Spline.GetLocalCarOffset(carAspect.LaneNumber);

            // TODO: spin rot if backwards
            var rot = Spline.EvaluateRotation(rs.Start, rs.End, directionalT);
            

            carAspect.T = carT;
            carAspect.Position = splinePos + offset;
            carAspect.Rotation = rot;
        }
    }

    [UpdateAfter(typeof(CarSpeedSystem))]
    [BurstCompile]
    partial struct CarEvaluateSplinePositionSystem : ISystem
    {
        ComponentDataFromEntity<RoadSegment> m_RoadSegmentFromEntity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_RoadSegmentFromEntity = state.GetComponentDataFromEntity<RoadSegment>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_RoadSegmentFromEntity.Update(ref state);

            var dt = state.Time.DeltaTime;

            var evaluatePositionOnSpline = new CarEvaluateSplinePositionJob
            {
                DT = dt,
                RoadSegmentFromEntity = m_RoadSegmentFromEntity
            };
            evaluatePositionOnSpline.ScheduleParallel();
        }
    }
}
