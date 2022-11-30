using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    [BurstCompile]
    partial struct SpeedController : IJobEntity
    {
        [ReadOnly] public TrainPositions TrainPositions;
        public float DeltaTime;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNextTrainID(ref NativeArray<int> indexes, int metroLineID, int trainAmount, int trainID)
        {
            var startIndex = indexes[metroLineID];
            var lastIndex = startIndex + trainAmount - 1;
            var nextIndexOnLine = trainID - 1;
            if (nextIndexOnLine < startIndex)
                nextIndexOnLine = lastIndex;
            return nextIndexOnLine;
        }

        void Execute(ref TrainSpeedControllerAspect train)
        {
            var nextTrainID = GetNextTrainID(ref TrainPositions.StartIndexForMetroLine, train.MetroLineID, train.AmountOfTrains, train.UniqueTrainID);
            var nextTrainPosition = TrainPositions.TrainsPositions[nextTrainID];
            var distance = math.distance(train.Position, nextTrainPosition);
            if (distance < 35f )
            {
                train.Speed = math.min(train.Speed - (0.25f * DeltaTime) * train.MaxSpeed, 0f);
            }
            else
            {
                var distanceToDestination = math.distance(train.Destination, train.Position);
                if (distanceToDestination > 20f)
                    train.Speed = math.min(train.Speed + (0.25f * DeltaTime) * train.MaxSpeed, train.MaxSpeed);
                else if (train.DestinationType == RailwayPointType.Platform)
                {
                    var minimalSpeed = distanceToDestination < 0.1f ? 0f: 0.05f * train.MaxSpeed;
                    train.Speed = math.max(train.Speed - (0.25f * DeltaTime) * train.MaxSpeed, minimalSpeed);
                }
            }
        }
    }

    [BurstCompile]
    partial struct TrainMovementJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<MetroLine> m_MetroLine;

        // Time cannot be directly accessed from a job, so DeltaTime has to be passed in as a parameter.
        public float DeltaTime;


        void Execute(ref TrainAspect train)
        {
            var direction = train.Destination - train.Position;
            //train.Train.ValueRW.DestinationDirection = direction;
            var trainDirection = train.Forward;
            //train.Train.ValueRW.Forward = trainDirection;
            var angle = Utility.SignedAngle(math.forward(), direction, math.up());
            //train.Train.ValueRW.Angle = angle;
            if (math.abs(angle) > 0.001f)
                train.Rotation = quaternion.RotateY(angle);

            if (train.DestinationType == RailwayPointType.Route)
            {
                var nextSuggestedPosition = train.Position + math.normalize(direction) * (DeltaTime * train.CurrentSpeed);
                var distanceToNextPosition = math.distance(nextSuggestedPosition, train.Position);
                var distanceToDestination = math.distance(train.Destination, train.Position);
                if (distanceToNextPosition > distanceToDestination)
                {
                    var metroLine = m_MetroLine[train.Train.ValueRO.MetroLine];
                    var nextDestination = metroLine.GetNextRailwayPoint(train.DestinationIndex);
                    train.Destination = nextDestination.Item1;
                    train.DestinationType = nextDestination.Item2;
                    train.DestinationIndex = nextDestination.Item3;
                }

                train.Position = nextSuggestedPosition;
            }
            else
            {
                var distanceToThePoint = math.lengthsq(direction);
                if (distanceToThePoint > 0.001f)
                    train.Position += math.normalize(direction) * (DeltaTime * train.CurrentSpeed);
            }
        }
    }


    [BurstCompile]
    public partial struct TrainMovementSystem : ISystem
    {
        ComponentLookup<MetroLine> m_MetroLine;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_MetroLine = state.GetComponentLookup<MetroLine>(true);
            state.RequireForUpdate<Train>();
            state.RequireForUpdate<TrainPositions>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var trainPositions = SystemAPI.GetSingleton<TrainPositions>();
            if(trainPositions.TrainsPositions.Length == 0)
                return;
            
            m_MetroLine.Update(ref state);

            var speedController = new SpeedController
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                TrainPositions = trainPositions
            };
            var trainMovementJob = new TrainMovementJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                m_MetroLine = m_MetroLine
            };

            var speedHandle = speedController.ScheduleParallel(state.Dependency);
            trainMovementJob.ScheduleParallel(speedHandle).Complete();
        }
    }
}