using Authoring;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Systems
{
    [BurstCompile]
    public partial struct CarSpawning : ISystem
    {
        [BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<Config>();
		}

        [BurstCompile]
		public void OnDestroy(ref SystemState state)
		{

		}

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<Config>();
            var random = new Random(1234);

            for (int i = 0; i < config.NumCars; i++)
            {
                // TODO: Avoid overlapping
                // TODO: Get actual distance in the track, and assign segment id accordingly
                /*
                public float Distance;
                public float Length;
                public float Speed;
                public float Acceleration;
                public float TrackLength;
                public int LaneNumber;
                public float LaneChangeClearance;
                public float4 Color;
                public int SegmentNumber;
                */

                var car = state.EntityManager.Instantiate(config.CarPrefab);
                state.EntityManager.SetComponentData(car, new Car()
                {
                    Distance = random.NextFloat(99.0f),
                    Length = 1.0f,
                    Speed = 0.1f,
                    Acceleration = 0.0f,
                    TrackLength = 1.0f,
                    LaneNumber = random.NextInt(4),
                    LaneChangeClearance = 1.5f,
                    Color = float4.zero,
                    SegmentNumber = 0
                });

            }

            state.Enabled = false;
        }
    }
}
