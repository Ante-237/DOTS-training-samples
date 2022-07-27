using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Random = Unity.Mathematics.Random;

[WithAll(typeof(BeeStateGathering))]
[BurstCompile]
partial struct BeeGatherJob : IJobEntity
{
    public float GatherRadius;
    [ReadOnly] public StorageInfoFromEntity TargetStorageInfo;
    [ReadOnly] public ComponentDataFromEntity<Translation> TargetTranslationComponentData;
    [ReadOnly] public ComponentDataFromEntity<BlueTeam> BlueTeamComponentData;
    public ComponentDataFromEntity<ResourceStateGrabbable> ResourceStateGrabbableComponentData;
    public ComponentDataFromEntity<ResourceStateGrabbed> ResourceStateGrabbedComponentData;
    public ComponentDataFromEntity<Gravity> GravityComponentData;
    public ComponentDataFromEntity<BeeStateGathering> BeeStateGatheringComponentData;
    public ComponentDataFromEntity<BeeStateReturning> BeeStateReturningComponentData;
    public Config config;
    public uint RandomSeed;

    void Execute(Entity entity, in Translation position, ref TargetPosition targetPosition, in EntityOfInterest entityOfInterest)
    {
        Entity targetEntity = entityOfInterest.Value;
        if (TargetStorageInfo.Exists(targetEntity))
        {
            if (TargetTranslationComponentData.HasComponent(targetEntity))
            {
                targetPosition.Value = TargetTranslationComponentData[targetEntity].Value;
                
                if (ResourceStateGrabbableComponentData.IsComponentEnabled(targetEntity))
                {
                    float dist = math.distance(position.Value, targetPosition.Value);

                    if (dist < GatherRadius)
                    {
                        // Set Resource states
                        ResourceStateGrabbableComponentData.SetComponentEnabled(targetEntity, false);
                        ResourceStateGrabbedComponentData.SetComponentEnabled(targetEntity, true);
                        GravityComponentData.SetComponentEnabled(targetEntity, false);

                        // Set Bee states
                        BeeStateGatheringComponentData.SetComponentEnabled(entity, false);
                        BeeStateReturningComponentData.SetComponentEnabled(entity, true);

                        var rand = Random.CreateFromIndex(RandomSeed + (uint)entity.Index);
                        float randY = rand.NextFloat(position.Value.y + 1.5f, config.PlayVolume.y);
                        float x = config.PlayVolume.x * 2;
                        x *= BlueTeamComponentData.HasComponent(entity) ? -1f : 1f;
                        targetPosition.Value = math.float3(x, randY, position.Value.z);
                    }
                }
            }

        }
        else
        {
            // do busted state stuff
            BeeStateGatheringComponentData.SetComponentEnabled(entity, false);
            BeeStateReturningComponentData.SetComponentEnabled(entity, true);

            targetPosition.Value = position.Value;
        }
    }
}

[BurstCompile]
public partial struct BeeGatheringSystem : ISystem
{
    private StorageInfoFromEntity storageInfo;
    private ComponentDataFromEntity<Translation> translationComponentData;
    private ComponentDataFromEntity<ResourceStateGrabbable> resourceStateGrabbableComponentData;
    private ComponentDataFromEntity<BlueTeam> blueTeamComponentData;
    private ComponentDataFromEntity<ResourceStateGrabbed> resourceStateGrabbedComponentData;
    private ComponentDataFromEntity<Gravity> gravityComponentData;
    private ComponentDataFromEntity<BeeStateGathering> beeStateGatheringComponentData;
    private ComponentDataFromEntity<BeeStateReturning> beeStateReturningComponentData;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        storageInfo = state.GetStorageInfoFromEntity();
        translationComponentData = state.GetComponentDataFromEntity<Translation>();
        resourceStateGrabbableComponentData = state.GetComponentDataFromEntity<ResourceStateGrabbable>();
        blueTeamComponentData = state.GetComponentDataFromEntity<BlueTeam>();
        resourceStateGrabbedComponentData = state.GetComponentDataFromEntity<ResourceStateGrabbed>();
        gravityComponentData = state.GetComponentDataFromEntity<Gravity>();
        beeStateGatheringComponentData = state.GetComponentDataFromEntity<BeeStateGathering>();
        beeStateReturningComponentData = state.GetComponentDataFromEntity<BeeStateReturning>();

    state.RequireForUpdate<Config>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        float gatherRadius = config.InteractionDistance;

        storageInfo.Update(ref state);
        translationComponentData.Update(ref state);
        resourceStateGrabbableComponentData.Update(ref state);
        blueTeamComponentData.Update(ref state);
        resourceStateGrabbedComponentData.Update(ref state);
        gravityComponentData.Update(ref state);
        beeStateGatheringComponentData.Update(ref state);
        beeStateReturningComponentData.Update(ref state); 

        var gatherJob = new BeeGatherJob()
        {
            GatherRadius = gatherRadius,
            TargetStorageInfo = storageInfo,
            TargetTranslationComponentData = translationComponentData,
            ResourceStateGrabbableComponentData = resourceStateGrabbableComponentData,
            BlueTeamComponentData = blueTeamComponentData,
            ResourceStateGrabbedComponentData = resourceStateGrabbedComponentData,
            GravityComponentData = gravityComponentData,
            BeeStateGatheringComponentData = beeStateGatheringComponentData,
            BeeStateReturningComponentData = beeStateReturningComponentData,
            config = config,
            RandomSeed = (uint)UnityEngine.Time.frameCount
        };

        gatherJob.Schedule();
    }
}
