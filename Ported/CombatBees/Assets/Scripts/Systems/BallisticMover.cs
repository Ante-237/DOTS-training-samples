using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class BallisticMover : SystemBase
{
    EndSimulationEntityCommandBufferSystem ecbs;
    BeginSimulationEntityCommandBufferSystem becbs;

    protected override void OnCreate()
    {
        base.OnCreate();
        ecbs = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        becbs = World
            .GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        this.RequireSingletonForUpdate<GlobalData>();
    }
  

    protected override void OnUpdate()
    {
        var globalDataEntity = GetSingletonEntity<GlobalData>();
        var globalData = GetComponent<GlobalData>(globalDataEntity);

        var time = Time.DeltaTime;
        float3 gravityVector = new float3(0, -2, 0);

        var becb = becbs.CreateCommandBuffer().AsParallelWriter();
        var ecb = ecbs.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAll<Ballistic>()
            .ForEach((ref Velocity velocity) => { velocity.Value += gravityVector * time; }).ScheduleParallel();

        Entities
            .WithAll<Ballistic>()
            .ForEach((ref Translation translation, in Velocity velocity) =>
            {
                translation.Value = translation.Value + velocity.Value * time;
            }).ScheduleParallel();

        Entities
            .WithAll<Ballistic>()
            .WithNone<Decay, InHive>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Velocity velocity, in AABB aabb) =>
            {
                var abspos = math.abs(translation.Value + aabb.center)+aabb.halfSize;
                if (abspos.x > globalData.BoundsMax.x)
                {
                    if (velocity.Value.x * translation.Value.x > 0.0f)
                    {
                        velocity.Value.x *= -0.5f;
                    }
                }
                
                if (abspos.z > globalData.BoundsMax.z)
                {
                    if (velocity.Value.z * translation.Value.z > 0.0f)
                    {
                        velocity.Value.z *= -0.5f;
                    }
                }
                
                var height = translation.Value.y + aabb.center.y - aabb.halfSize.y;
            
            if (height < globalData.BoundsMin.y)
            {
                translation.Value.y = globalData.BoundsMin.y + aabb.halfSize.y - aabb.center.y;
                ecb.RemoveComponent<Ballistic>(entityInQueryIndex, entity);
                if (!HasComponent<Food>(entity))
                    ecb.AddComponent(entityInQueryIndex, entity, new Decay());
            }
        }).ScheduleParallel();

        Entities
        .WithAll<Ballistic, Food>()
        .WithNone<Decay>()
        .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in AABB aabb, in TargetedBy targetedby) =>
        {
            var height = translation.Value.y + aabb.center.y - aabb.halfSize.y;

            if (height < globalData.BoundsMin.y)
            {
                // Despawn the food object
                ecb.DestroyEntity(entityInQueryIndex, entity);

                var explosion = becb.Instantiate(entityInQueryIndex, globalData.ExplosionPrefab);
                becb.SetComponent<Translation>(entityInQueryIndex, explosion, translation);

                for (int i = 0; i < globalData.BeeExplosionCount; ++i)
                {
                    var bee = becb.Instantiate(entityInQueryIndex, globalData.BeePrefab);
                    becb.SetComponent<Translation>(entityInQueryIndex, bee, translation);
                    BeeSpawner.SetBees(bee, entityInQueryIndex, becb, GetComponent<TeamID>(entity));
                }
            }
        }).ScheduleParallel();

        ecbs.AddJobHandleForProducer(Dependency);
        becbs.AddJobHandleForProducer(Dependency);
    }
}