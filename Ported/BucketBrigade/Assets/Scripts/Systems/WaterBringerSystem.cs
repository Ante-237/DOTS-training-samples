﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
partial struct WaterBringerSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FireFighterConfig>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        var ffConfig = SystemAPI.GetSingleton<FireFighterConfig>();
        
        // Creating an EntityCommandBuffer to defer the structural changes required by instantiation.
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        var allStartPositions = new NativeArray<float2>(ffConfig.LinesCount, Allocator.TempJob);
        var allEndPositions = new NativeArray<float2>(ffConfig.LinesCount, Allocator.TempJob);
        foreach (var fireFighterLine in SystemAPI.Query<RefRO<FireFighterLine>>())
        {
            allStartPositions[fireFighterLine.ValueRO.LineId] = fireFighterLine.ValueRO.StartPosition;
            allEndPositions[fireFighterLine.ValueRO.LineId] = fireFighterLine.ValueRO.EndPosition;
        }

        // Creating an instance of the job.
        var waterBringerFindNewTarget = new WaterBringerFindNewTarget
        {
            StartPositions = allStartPositions,
            EndPositions = allEndPositions,
            ECB = ecb
        };

        // Schedule execution in a single thread, and do not block main thread.
        waterBringerFindNewTarget.Schedule();
    }
}

// Requiring the Shooting tag component effectively prevents this job from running
// for the tanks which are in the safe zone.
[WithAll(typeof(WaterBringer))]
[BurstCompile]
partial struct WaterBringerFindNewTarget : IJobEntity
{
    [ReadOnly]
    [DeallocateOnJobCompletion]
    public NativeArray<float2> StartPositions;
    
    [ReadOnly]
    [DeallocateOnJobCompletion]
    public NativeArray<float2> EndPositions;
    
    public EntityCommandBuffer ECB;

    // Note that the TurretAspects parameter is "in", which declares it as read only.
    // Making it "ref" (read-write) would not make a difference in this case, but you
    // will encounter situations where potential race conditions trigger the safety system.
    // So in general, using "in" everywhere possible is a good principle.
    void Execute(in Entity entity)
    {
        
        ECB.SetComponent(entity, new Target{ Value =  new float2 (10,10 )});
    }
}