using Components;
using Unity.Burst;
using Metro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial struct PassangerOnTrainSystem : ISystem
{
    private Random random;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        random = Random.CreateFromIndex(930);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;
        var stationConfig = SystemAPI.GetSingleton<StationConfig>();
        var config = SystemAPI.GetSingleton<Config>();

        // onboarding
        foreach (var (train, trainTransform, trainEntity) in
                 SystemAPI.Query<RefRO<Train>, RefRO<LocalTransform>>()
                 .WithAll<LoadingComponent>()
                 .WithEntityAccess())
        {
            foreach (var (passengerOnboardedComp, transform, passenger, travelInfo, passengerEntity) in
                     SystemAPI.Query<RefRW<PassengerOnboarded>, RefRW<LocalTransform>, RefRW<PassengerComponent>, RefRO<PassengerTravel>>()
                         .WithNone<PassengerWalkingToSeat>()
                         .WithEntityAccess())
            {
                if (passengerOnboardedComp.ValueRO.StationTrackPointIndex == train.ValueRO.TrackPointIndex &&
                    passenger.ValueRO.TrainId != train.ValueRO.TrainId &&
                    travelInfo.ValueRO.LineID == train.ValueRO.LineID &&
                    travelInfo.ValueRO.OnPlatformA == train.ValueRO.OnPlatformA)
                {
                    // the passenger is on the same station as the train
                    // store the trainID
                    passenger.ValueRW.TrainId = train.ValueRO.TrainId;
                    
                    // find closest seat
                    DynamicBuffer<SeatingComponentElement> seatBuffer = state.EntityManager.GetBuffer<SeatingComponentElement>(trainEntity);
                    if (!passenger.ValueRW.HasSeat)
                    {
                        var relativePosition = transform.ValueRO.Position - trainTransform.ValueRO.Position;
                        var closestSeatIndex = GetClosestSeatIndex(seatBuffer, relativePosition);
                        if (closestSeatIndex >= 0)
                        {
                            passenger.ValueRW.SeatPosition = seatBuffer[closestSeatIndex].SeatPosition;
                            passenger.ValueRW.SeatIndex = closestSeatIndex;
                            passenger.ValueRW.HasSeat = true;

                            var seat = seatBuffer.ElementAt(closestSeatIndex);
                            seat.Occupied = true;
                            seatBuffer[closestSeatIndex] = seat;

                            passenger.ValueRW.RelativePosition = relativePosition;
                        }
                    }

                    if (passenger.ValueRW.HasSeat)
                    {
                        em.SetComponentEnabled<PassengerWalkingToSeat>(passengerEntity, true);
                    }
                    else
                    {
                        // Offboarded
                        em.SetComponentEnabled<PassengerOnboarded>(passengerEntity, false);
                        em.SetComponentEnabled<PassengerOffboarded>(passengerEntity, true);
                    }
                }
            }
        }

        // onboard moving
        foreach (var (train, trainTransform) in
                 SystemAPI.Query<RefRO<Train>, RefRO<LocalTransform>>())
        {
            foreach (var (passengerOnboardedComp, transform, passenger) in
                SystemAPI.Query<RefRW<PassengerOnboarded>, RefRW<LocalTransform>, RefRW<PassengerComponent>>())
            {
                if ( passenger.ValueRW.TrainId == train.ValueRO.TrainId)
                {
                    transform.ValueRW.Position = passenger.ValueRW.RelativePosition + trainTransform.ValueRO.Position;
                    passenger.ValueRW.HasJustUnloaded = true;
                }
            }
        }
        
        //offboarding
        foreach (var (train, trainTransform, trainEntity) in SystemAPI.Query<RefRO<Train>, RefRO<LocalTransform>>().WithAll<UnloadingComponent>().WithEntityAccess())
        {
            if (train.ValueRO.Duration < 0.01f)
            {
                DynamicBuffer<SeatingComponentElement> seatBuffer = state.EntityManager.GetBuffer<SeatingComponentElement>(trainEntity);
                for (int i = 0; i < seatBuffer.Length; i++)
                {
                    if (random.NextBool())
                    {
                        seatBuffer.ElementAt(i).Occupied = false;   
                    }
                }

                foreach (var (transform, passenger, entity) in
                         SystemAPI.Query< RefRW<LocalTransform>, RefRW<PassengerComponent>>()
                             .WithAll<PassengerOnboarded>()
                             .WithNone<PassengerWalkingToSeat>()
                             .WithEntityAccess())
                {
                    if (passenger.ValueRW.TrainId == train.ValueRO.TrainId)
                    {
                        if (passenger.ValueRO.HasSeat && !seatBuffer.ElementAt(passenger.ValueRO.SeatIndex).Occupied)
                        {
                            passenger.ValueRW.HasSeat = false;
                            passenger.ValueRW.SeatIndex = -1;

                            em.SetComponentEnabled<PassengerWalkingToDoor>(entity, true);
                            SetPassengerTargetExitPosition(stationConfig, config, passenger, trainTransform.ValueRO, train.ValueRO.OnPlatformA);
                        }
                    }
                }
                
                foreach (var (transform, passenger, entity) in
                         SystemAPI.Query< RefRW<LocalTransform>, RefRW<PassengerComponent>>()
                             .WithAll<PassengerWaitingToExit>()
                             .WithEntityAccess())
                {
                    if (passenger.ValueRW.TrainId == train.ValueRO.TrainId)
                    {
                        transform.ValueRW.Position = trainTransform.ValueRO.Position + passenger.ValueRO.ExitPosition;

                        var travelInfo = em.GetComponentData<PassengerTravel>(entity);
                        travelInfo.Station = train.ValueRO.StationEntity;
                        em.SetComponentData(entity, travelInfo);
                        em.SetComponentEnabled<PassengerOnboarded>(entity, false);
                        em.SetComponentEnabled<PassengerWaitingToExit>(entity, false);
                        em.SetComponentEnabled<PassengerOffboarded>(entity, true);
                    }
                }
            }
        }
    }

    public void SetPassengerTargetExitPosition(StationConfig stationConfig, Config config, RefRW<PassengerComponent> passenger, LocalTransform trainLocalTransform, bool onPlatformA)
    {
        var halfSpan = config.CarriageLength * (stationConfig.NumQueingPointsPerPlatform - 1f) / 2f;
        var minDistance = float.MaxValue;
        float3 targetExitPosition = float3.zero;

        var negX = onPlatformA ? 1.0f : -1.0f;
        for (int i = 0; i < stationConfig.NumQueingPointsPerPlatform; i++)
        {
            var exitPosition = negX * 0.67f * trainLocalTransform.Right() + 1.6f * trainLocalTransform.Up() + 
                               (i * config.CarriageLength - halfSpan) * trainLocalTransform.Forward();
            var distToExit = math.distance(passenger.ValueRO.SeatPosition, exitPosition);
            
            if (distToExit <= minDistance)
            {
                minDistance = distToExit;
                targetExitPosition = exitPosition;
            }

            passenger.ValueRW.ExitPosition = targetExitPosition;
        }
        
    }
    
    public int GetClosestSeatIndex(DynamicBuffer<SeatingComponentElement> seatBuffer, float3 passengerRelativePosition)
    {
        var minDist = float.MaxValue;
        var seatIndex = -1;
        
        for (int i = 0; i < seatBuffer.Length; i++)
        {
            if (seatBuffer[i].Occupied)
            {
                continue;
            }

            var dist = math.distance(passengerRelativePosition, seatBuffer[i].SeatPosition);

            if (dist < minDist)
            {
                seatIndex = i;
                minDist = dist;
            }
        }
        
        return seatIndex;
    }
}


