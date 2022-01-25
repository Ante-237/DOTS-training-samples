using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Intentionally spawning the particles in the floor.
/// In the demo, the cubes spawn all over the place,
/// but still, these particles will be attracted to the tornado. 
/// </summary>
public partial class TornadoSpanwerSystem : SystemBase
{
    const int padding = 4;
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var random = new Random((uint)DateTime.Now.Millisecond+1);
        var floor = GetSingletonEntity<Floor>();
        var scale = GetComponent<NonUniformScale>(floor).Value*padding;
        
        Entities
            .ForEach((Entity entity, in TornadoSpawner spawner) =>
            {
                ecb.DestroyEntity(entity);
                int particleCount = spawner.particleCount;
                for (int i = 0; i < particleCount; i++)
                {
                    var instance = ecb.Instantiate(spawner.particlePrefab);

                    var randomXZPos = random.NextFloat3(scale*-1 ,scale);
                    var translation = new Translation {Value = new float3(randomXZPos.x, 0, randomXZPos.z)};
                    ecb.SetComponent(instance, translation);
                }
            }).Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
