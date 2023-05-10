﻿using Components;
using Metro;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public partial struct TrainSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        state.RequireForUpdate<StationIDComponent>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        var trains = CollectionHelper.CreateNativeArray<Entity>(1, Allocator.Temp);

        var em = state.EntityManager;
        em.Instantiate(config.TrainEntity, trains);

        foreach (var (transform, train, entity) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRW<Train>>()
                     .WithEntityAccess())
        {
            var track = SystemAPI.GetSingletonBuffer<TrackPoint>(true);

            train.ValueRW.Offset = transform.ValueRO.Position;
            
            transform.ValueRW.Position = track[0].Position;

            em.SetComponentEnabled<EnRouteComponent>(entity, true);
            em.SetComponentEnabled<LoadingComponent>(entity, false);
            em.SetComponentEnabled<DepartingComponent>(entity, false);
        }
        state.Enabled = false;
    }
}
