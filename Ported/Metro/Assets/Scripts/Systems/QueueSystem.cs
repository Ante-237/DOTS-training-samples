using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct QueueSystem : ISystem
{
    Random m_Random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_Random = Random.CreateFromIndex(1234);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //TODO: get train from platform
        bool areDoorsOpen = false;
        foreach (var train in SystemAPI.Query<RefRW<Train>>())
        {
            areDoorsOpen |= train.ValueRW.State == TrainState.Boarding;
        }
        foreach (var queue in SystemAPI.Query<RefRW<QueueState>>())
        {
            queue.ValueRW.IsOpen = areDoorsOpen;
        }

        NativeList<Entity> queuesToUpdate = new(Allocator.Temp);

        foreach (var (queueingData, targetDestination, seatReservation, destinationAspect) in
                 SystemAPI.Query<RefRW<QueueingData>, RefRW<TargetDestination>, RefRW<SeatReservation>, DestinationAspect>())
        {
            if (queueingData.ValueRO.TargetQueue != Entity.Null)
            {
                var queue = SystemAPI.GetComponent<Queue>(queueingData.ValueRO.TargetQueue);
                var queueState = SystemAPI.GetComponent<QueueState>(queueingData.ValueRO.TargetQueue);
                var queueTransform = SystemAPI.GetComponent<WorldTransform>(queueingData.ValueRO.TargetQueue);

                targetDestination.ValueRW.TargetPosition = queueTransform.Position + queueingData.ValueRO.PositionInQueue * queue.QueueDirection;

                if (queueState.IsOpen && queueingData.ValueRO.PositionInQueue == 0 && destinationAspect.IsAtDestination())
                {
                    Entity availableSeatEntity = FindAvailableSeat(ref state, queueState.FacingCarriage);
                    if (availableSeatEntity != Entity.Null)
                    {
                        var seatTransform = SystemAPI.GetComponent<WorldTransform>(availableSeatEntity);
                        targetDestination.ValueRW.TargetPosition = seatTransform.Position;
                        seatReservation.ValueRW.TargetSeat = availableSeatEntity;

                        SystemAPI.SetComponent(availableSeatEntity, new Seat(){ IsTaken = true });

                        queuesToUpdate.Add(queueingData.ValueRO.TargetQueue);
                        queueingData.ValueRW.TargetQueue = Entity.Null;
                    }
                }
            }
        }

        foreach (var (queue, queueEntity) in SystemAPI.Query<RefRW<QueueState>>().WithEntityAccess())
        {
            if (queuesToUpdate.Contains(queueEntity))
            {
                --queue.ValueRW.QueueSize;
            }
        }

        foreach (var (queueingData, queueEntity) in SystemAPI.Query<RefRW<QueueingData>>().WithEntityAccess())
        {
            if (queuesToUpdate.Contains(queueingData.ValueRW.TargetQueue))
            {
                --queueingData.ValueRW.PositionInQueue;
            }
        }

    }

    private Entity FindAvailableSeat(ref SystemState state, Entity carriage)
    {
        var seats = SystemAPI.GetBuffer<CarriageSeat>(carriage);
        foreach (var carriageSeat in seats)
        {
            var seat = SystemAPI.GetComponent<Seat>(carriageSeat.Seat);
            if (!seat.IsTaken)
            {
                return carriageSeat.Seat;
            }
        }
        return Entity.Null;
    }
}
