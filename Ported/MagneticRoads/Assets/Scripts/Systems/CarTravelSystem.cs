using Aspects;
using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    partial struct CarTravelSystem : ISystem
    {
        ComponentDataFromEntity<Track> m_TrackComponentFromEntity;

        [BurstCompile] public void OnCreate(ref SystemState state)
        {
            m_TrackComponentFromEntity = state.GetComponentDataFromEntity<Track>(true);
        }

        [BurstCompile] public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile] 
        public void OnUpdate(ref SystemState state)
        {
            m_TrackComponentFromEntity.Update(ref state);
            
            var dt = state.Time.DeltaTime;
            foreach (var car in SystemAPI.Query<CarAspect>())
            {
                car.T = math.clamp(car.T + dt, 0, 1);
                m_TrackComponentFromEntity.TryGetComponent(car.Track, out Track track);

                car.Position = track.Evaluate(car.T);
            }
        }


        
    }
}