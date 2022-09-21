﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
partial struct FoodSpawnerSystem : ISystem
{
    private EntityQuery NestQuery;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        NestQuery = SystemAPI.QueryBuilder().WithAll<Area, Faction>().Build();
        
        // This system should not run before the Config singleton has been loaded.
        state.RequireForUpdate<BeeConfig>();
        state.RequireForUpdate(NestQuery);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<BeeConfig>();
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        var nestEntities = NestQuery.ToEntityArray(Allocator.Temp);

        var combinedJobHandle = new JobHandle();
        foreach (var nest in nestEntities)
        {
            var nestFaction = state.EntityManager.GetComponentData<Faction>(nest);
            if (nestFaction.Value != (int)Factions.None)
            {
                continue;
            }

            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var nestArea = state.EntityManager.GetComponentData<Area>(nest);
            var transform = state.EntityManager.GetComponentData<LocalToWorldTransform>(config.food);
            var foodSpawnJob = new SpawnJob
            {
                // Note the function call required to get a parallel writer for an EntityCommandBuffer.
                Aabb = nestArea.Value,
                ECB = ecb.AsParallelWriter(),
                Prefab = config.food,
                InitTransform = transform,
                InitFaction = (int)Factions.None,
            };
            var jobHandle = foodSpawnJob.Schedule(config.beeCount, 64, state.Dependency);
            combinedJobHandle = JobHandle.CombineDependencies(jobHandle, combinedJobHandle);

        }

        // establish dependency between the spawn job and the command buffer to ensure the spawn job is completed
        // before the command buffer is played back.
        state.Dependency = combinedJobHandle;

        // force disable system after the first update call
        state.Enabled = false;
    }
}