using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(OmnibotMovingSystem))]
[BurstCompile]
public partial struct OmniBucketSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var omnibotLookup = SystemAPI.GetComponentLookup<Omnibot>();
        var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>();

        foreach (var (bucket, localTransform, pTMatrix) in SystemAPI
                     .Query<RefRW<OmniBucket>, RefRW<LocalTransform>, RefRW<PostTransformMatrix>>())
        {
            var omnibot = omnibotLookup[bucket.ValueRO.TargetOmniBotEntity];
            var omnibotPosition = localTransformLookup[bucket.ValueRO.TargetOmniBotEntity].Position;

            // Assign OMniBot to Bucket
            localTransform.ValueRW.Position = omnibotPosition + new float3(0,1,0);
            pTMatrix.ValueRW.Value = float4x4.Scale(new float3(
                1, omnibot.CurrentWaterCarryingVolume / omnibot.MaxWaterCapacity, 1));
        }
    }
}