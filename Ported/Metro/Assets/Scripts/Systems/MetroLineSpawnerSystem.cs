using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    [UpdateAfter(typeof(TrainMovementSystem))]
    public partial struct MetroLineSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MetroLine>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var trainConfig = SystemAPI.GetSingleton<TrainConfig>();
            var uniqueMetroIndex = 0;
            foreach (var (metroLine, entity) in SystemAPI.Query<MetroLine>().WithEntityAccess())
            {
                var id = SystemAPI.GetComponent<MetroLineID>(entity);
                
                var countTrains = 0;
                for (int i = 0; i < metroLine.RailwayPositions.Length; i++)
                {
                    if (metroLine.RailwayTypes[i] != RailwayPointType.Route)
                        continue;
                    countTrains++;
                }

                for (int i = 0; i < metroLine.RailwayPositions.Length; i++)
                {
                    if (metroLine.RailwayTypes[i] != RailwayPointType.Route)
                        continue;
                    var train = ecb.Instantiate(trainConfig.TrainPrefab);
                    ecb.SetComponent(train, LocalTransform.FromPositionRotation(metroLine.RailwayPositions[i], metroLine.RailwayRotations[i]));
                    var nextIndex = i - 1 < 0 ? metroLine.RailwayPositions.Length - 1 : i - 1;
                    ecb.SetComponent(train, new Train
                    {
                        UniqueTrainID = uniqueMetroIndex,
                        AmountOfTrainsOnMetroLine = countTrains,
                        DestinationIndex = nextIndex,
                        Destination = metroLine.RailwayPositions[nextIndex],
                        DestinationType = metroLine.RailwayTypes[nextIndex],
                        State = TrainState.EnRoute,
                        MetroLine = entity
                    });
                    ecb.SetComponent(train, new MetroLineID
                    {
                        ID = id.ID
                    });
                    uniqueMetroIndex++;
                }
            }

            state.Enabled = false;
        }
    }
}