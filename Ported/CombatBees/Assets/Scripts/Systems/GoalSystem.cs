using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Mathf = UnityEngine.Mathf;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class GoalSystem : SystemBase
{
    EntityCommandBufferSystem beginSimulationEntityCommandBufferSystem;
    EntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    EntityQuery[] teamTargets;

    protected override void OnCreate()
    {
        beginSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var spawner = GetSingleton<SpawnData>();
        var particles = GetSingleton<ParticleSettings>();

        var beginFrameEcb = beginSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var endFrameEcb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var gsv = GlobalSystemVersion;

        Dependency = Entities
            .WithAll<Components.Resource>()
            .WithNone<Components.KinematicBody>()
            .ForEach((Entity entity, int entityInQueryIndex, in Translation translation) =>
            {
                if ((Mathf.Abs(translation.Value.y) >= (PlayField.size.y * .5f) - math.EPSILON) &&
                    (Mathf.Abs(translation.Value.x) > PlayField.size.x * .4f))
                {
                    int team = 0;
                    if (translation.Value.x > 0f)
                    {
                        team = 1;
                    }

                    var random = new Random(gsv * (uint)entity.Index);
                    beginFrameEcb.DestroyEntity(entityInQueryIndex, entity);

                    // Spawn new bees and particles
                    for (int i = 0; i < 3; ++i)
                    {
                        Instantiation.Bee.Instantiate(endFrameEcb, entityInQueryIndex, spawner.BeePrefab, translation.Value,
                            random.NextFloat(0.25f, 0.5f), team);
                        ParticleSystem.SpawnParticle(beginFrameEcb, entityInQueryIndex, particles.Particle, random,
                            translation.Value, ParticleComponent.ParticleType.SpawnFlash, float3.zero, 6f, 5);
                    }
                }
            }).ScheduleParallel(Dependency);

        beginSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}