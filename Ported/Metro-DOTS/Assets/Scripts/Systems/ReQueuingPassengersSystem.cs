using Components;
using Metro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(PassengerSystemGroup))]
// [UpdateAfter(typeof(PassengerMovingOnTrainSystem))]
public partial struct ReQueuingPassengersSystem : ISystem
{
    private Random random;
    private float3 Y;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        random = Random.CreateFromIndex(12314);
        Y = new float3(0, 1f, 0);
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();

        // assign off boarded passenger new queues
        foreach (var (travelInfo, passenger) in
                 SystemAPI.Query<RefRW<PassengerTravel>>()
                     .WithAll<PassengerOffboarded>()
                     .WithEntityAccess())
        {
            var station = travelInfo.ValueRO.Station;
            var queues = state.EntityManager.GetBuffer<StationQueuesElement>(station);

            var nextQueueId = (int)(random.NextFloat() * queues.Length);

            var queue = queues.ElementAt(nextQueueId).Queue;
            var queueInfo = state.EntityManager.GetComponentData<QueueComponent>(queue);

            if (queueInfo.QueueLength < config.MaxPassengerPerQueue)
            {
                state.EntityManager.SetComponentEnabled<PassengerOffboarded>(passenger, false);
                state.EntityManager.SetComponentEnabled<PassengerWalkingToQueue>(passenger, true);
                travelInfo.ValueRW.Queue = queue;
                travelInfo.ValueRW.OnPlatformA = queueInfo.OnPlatformA;
            }

        }
        
        // move passengers towards new queues
        foreach (var (travelInfo, passengerLocalTransform, passenger) in
                 SystemAPI.Query<RefRW<PassengerTravel>, RefRW<LocalTransform>>()
                     .WithAll<PassengerWalkingToQueue>()
                     .WithEntityAccess())
        {
            var queue = travelInfo.ValueRO.Queue;
            var queueLocalTransform = state.EntityManager.GetComponentData<LocalTransform>(queue);
            var queueInfo = state.EntityManager.GetComponentData<QueueComponent>(queue);
            var queueTailPosition =
                queueLocalTransform.Position - queueInfo.QueueLength * queueLocalTransform.Forward() * config.DistanceBetweenPassengers;
        
            var passengerPosition = passengerLocalTransform.ValueRO.Position;
        
            var toQueueTail = queueTailPosition - passengerPosition;
            var distToQueueTail = math.length(toQueueTail);
        
            if (distToQueueTail < 0.01f)
            {
                var queuePassengers = state.EntityManager.GetBuffer<QueuePassengers>(queue);
                queuePassengers.ElementAt((queueInfo.StartIndex + queueInfo.QueueLength) % config.MaxPassengerPerQueue) = new QueuePassengers{Passenger =  passenger};
                queueInfo.QueueLength++;
                state.EntityManager.SetComponentData<QueueComponent>(queue, queueInfo);
                state.EntityManager.SetComponentEnabled<PassengerWalkingToQueue>(passenger, false);
        
                passengerLocalTransform.ValueRW.Rotation = queueLocalTransform.Rotation;
            }
            else
            {
                var toQueueTailDirection = math.normalize(toQueueTail);
                passengerLocalTransform.ValueRW.Position += toQueueTailDirection * config.PassengerSpeed * SystemAPI.Time.DeltaTime;
        
                var rotationAngle = math.acos(math.dot(Y, toQueueTailDirection));
                passengerLocalTransform.ValueRW.Rotation = quaternion.RotateY(rotationAngle);
            }
            
        }

        // var movePassengerToNewQueuesJob = new MovePassengersToNewQueuesJob
        // {
        //     Config = config,
        //     PassengerSpeed = SystemAPI.Time.DeltaTime * config.PassengerSpeed,
        //     QueueTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
        //     QueueLookup = SystemAPI.GetComponentLookup<QueueComponent>(),
        //     QueuePassengersLookup = SystemAPI.GetBufferLookup<QueuePassengers>(),
        //     Y = Y
        // };
        // state.Dependency = movePassengerToNewQueuesJob.Schedule(state.Dependency);
    }
}

// [BurstCompile]
// public partial struct MovePassengersToNewQueuesJob : IJobEntity
// {
//     public Config Config;
//     public float PassengerSpeed;
//     public ComponentLookup<LocalTransform> QueueTransformLookup;
//     public ComponentLookup<QueueComponent> QueueLookup;
//     public BufferLookup<QueuePassengers> QueuePassengersLookup;
//     public float3 Y;
//
//     public void Execute(in PassengerTravel travelInfo, ref LocalTransform passengerLocalTransform,
//         EnabledRefRW<PassengerWalkingToQueue> walkingState, Entity passengerEntity)
//     {
//         var queue = travelInfo.Queue;
//         var queueLocalTransform = QueueTransformLookup[queue];
//         var queueInfo = QueueLookup[queue];
//         var queueTailPosition =
//             queueLocalTransform.Position - queueInfo.QueueLength * queueLocalTransform.Forward() * Config.DistanceBetweenPassengers;
//
//         var passengerPosition = passengerLocalTransform.Position;
//
//         var toQueueTail = queueTailPosition - passengerPosition;
//         var distToQueueTail = math.length(toQueueTail);
//
//         if (distToQueueTail < 0.01f)
//         {
//             var queuePassengers = QueuePassengersLookup[queue];
//             queuePassengers.ElementAt((queueInfo.StartIndex + queueInfo.QueueLength) % Config.MaxPassengerPerQueue) = 
//                 new QueuePassengers{Passenger = passengerEntity};
//             queueInfo.QueueLength++;
//             QueueLookup[queue] = queueInfo;
//             walkingState.ValueRW = false;
//             
//             passengerLocalTransform.Rotation = queueLocalTransform.Rotation;
//         }
//         else
//         {
//             var toQueueTailDirection = math.normalize(toQueueTail);
//             passengerLocalTransform.Position += toQueueTailDirection * PassengerSpeed;
//
//             var rotationAngle = math.acos(math.dot(Y, toQueueTailDirection));
//             passengerLocalTransform.Rotation = quaternion.RotateY(