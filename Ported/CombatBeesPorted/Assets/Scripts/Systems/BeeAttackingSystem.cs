using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[WithAll(typeof(BeeStateAttacking))]
[CreateAfter(typeof(BeeGatheringSystem))]
[BurstCompile]
partial struct BeeAttackJob : IJobEntity
{
    public float AttackRadius;
    [ReadOnly] public StorageInfoFromEntity TargetStorageInfo;
    [ReadOnly] public ComponentDataFromEntity<Translation> TargetTranslationComponentData;
    public EntityCommandBuffer.ParallelWriter ECB;
    public uint RandomSeed;

    void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in Translation position, ref TargetPosition targetPosition, in EntityOfInterest entityOfInterest)
    {
        Entity targetEntity = entityOfInterest.Value;
        if (TargetStorageInfo.Exists(targetEntity))
        {
            if (TargetTranslationComponentData.HasComponent(targetEntity))
            {
                targetPosition.Value = TargetTranslationComponentData[targetEntity].Value;
                float dist = math.distance(position.Value, targetPosition.Value);

                if (dist < AttackRadius)
                {
                    // Set Bee State
                    ECB.SetComponentEnabled<BeeStateDead>(chunkIndex, targetEntity, true);
                    ECB.SetComponentEnabled<BeeStateAttacking>(chunkIndex, targetEntity, false);
                    ECB.SetComponentEnabled<BeeStateGathering>(chunkIndex, targetEntity, false);
                    ECB.SetComponentEnabled<BeeStateReturning>(chunkIndex, targetEntity, false);
                    ECB.SetComponentEnabled<BeeStateIdle>(chunkIndex, targetEntity, false);
                    
                    ECB.SetComponentEnabled<BeeStateAttacking>(chunkIndex, entity, false);
                    ECB.SetComponentEnabled<BeeStateIdle>(chunkIndex, entity, true);
                    
                    Debug.Log("Killed");
                }
            }
        }
        else
        {
            ECB.SetComponentEnabled<BeeStateAttacking>(chunkIndex, entity, false);
            ECB.SetComponentEnabled<BeeStateIdle>(chunkIndex, entity, true);

            targetPosition.Value = position.Value;
        }
    }
}

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[BurstCompile]
public partial struct BeeAttackingSystem : ISystem
{
    private StorageInfoFromEntity storageInfo;
    private ComponentDataFromEntity<Translation> translationComponentData;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
        
        storageInfo = state.GetStorageInfoFromEntity();
        translationComponentData = state.GetComponentDataFromEntity<Translation>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        float attackRadius = config.InteractionDistance;
        
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        storageInfo.Update(ref state);
        translationComponentData.Update(ref state);

        var attackJob = new BeeAttackJob()
        {
            ECB = ecb,
            AttackRadius = attackRadius,
            TargetStorageInfo = storageInfo,
            TargetTranslationComponentData = translationComponentData,
        };

        attackJob.ScheduleParallel();
    }
}
