﻿using System.Security.Cryptography.X509Certificates;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
partial struct BeeSpawnSystem : ISystem
{
    float aggressiveThreshold;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        aggressiveThreshold = 0.6f; // Some hardcoded value. If the bee's scale is above it, the bee will be aggressive and attack.
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        var beeSizeHalfRange = (config.maximumBeeSize - config.minimumBeeSize) * .5f;
        var beeSizeMiddle = config.minimumBeeSize + beeSizeHalfRange;
        foreach(var (hive, team) in SystemAPI.Query<RefRO<Hive>, Team>())
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var bees = new NativeArray<Entity>(config.startBeeCount, Allocator.Temp);
            ecb.Instantiate(config.beePrefab, bees);
            
            ecb.SetSharedComponent(bees, new Team()
            {
                number = team.number
            });
            var hiveValue = hive.ValueRO;
            var color = new URPMaterialPropertyBaseColor { Value = (Vector4)hiveValue.color };

            foreach (var bee in bees)
            {
                ecb.SetComponent(bee, color);
                var pos = hiveValue.boundsPosition;
                pos.y = bee.Index;
                var position = hiveValue.boundsPosition;
                position.x += noise.cnoise(pos / 10f) * hiveValue.boundsExtents.x;
                position.y += noise.cnoise(pos / 11f) * hiveValue.boundsExtents.y;
                position.z += noise.cnoise(pos / 12f) * hiveValue.boundsExtents.z;
                var scaleRandom = math.clamp(noise.cnoise(pos / 13f) * 2f, -1f, 1f);
                var scaleDelta = scaleRandom * beeSizeHalfRange;
                var scale = math.clamp(scaleDelta + beeSizeMiddle,
                    config.minimumBeeSize, config.maximumBeeSize);
                Debug.Log($"scaledelta {scaleDelta}, scale {scale}, halfrange {beeSizeHalfRange}, middle {beeSizeMiddle}");
                ecb.SetComponent(bee, new LocalTransform
                {
                    Position = position,
                    Scale = scale
                });

                ecb.SetComponent(bee, new BeeState
                {
                    beeState = scale > aggressiveThreshold ? BeeStateEnumerator.Attacking : BeeStateEnumerator.Gathering
                });
            }
        }

        state.Enabled = false;
    }
}