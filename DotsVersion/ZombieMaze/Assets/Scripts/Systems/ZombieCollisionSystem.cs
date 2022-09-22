using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
partial struct ZombieCollisionSystem : ISystem
{
    EntityQuery Query;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MazeConfig>();
        state.RequireForUpdate<PlayerData>();

        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<Zombie>();
        Query = state.GetEntityQuery(builder);
    }

    // Every function defined by ISystem has to be implemented even if empty.
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    // See note above regarding the [BurstCompile] attribute.
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NativeArray<ArchetypeChunk> chunks = Query.ToArchetypeChunkArray(Allocator.Temp);

        MazeConfig mazeConfig = SystemAPI.GetSingleton<MazeConfig>();
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerData>();
        PlayerData playerData = SystemAPI.GetSingleton<PlayerData>();
        TransformAspect playerTransform = SystemAPI.GetAspectRW<TransformAspect>(playerEntity);
        float3 playerPosition = playerTransform.Position;

        ComponentTypeHandle<LocalToWorld> positionHandle = state.EntityManager.GetComponentTypeHandle<LocalToWorld>(true);
        EntityTypeHandle entityHandle = state.EntityManager.GetEntityTypeHandle();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        for (int i = 0; i < chunks.Length; ++i)
        {
            var chunk = chunks[i];
            var positions = chunk.GetNativeArray(positionHandle);
            var entities = chunk.GetNativeArray(entityHandle);

            for (int j = 0; j < chunk.Count; ++j)
            {
                var distance = math.distancesq(playerPosition, positions[j].Position);
                if (distance < 0.25f)
                {
                    playerTransform.Position = new float3(playerData.spawnerPos.x, playerTransform.Position.y, playerData.spawnerPos.z);


                    mazeConfig.ZombiesToSpawn = 1;
                    SystemAPI.SetSingleton<MazeConfig>(mazeConfig);
                    ecb.DestroyEntity(entities[j]);
                    break;
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}