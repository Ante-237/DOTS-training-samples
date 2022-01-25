﻿using Unity.Entities;
using Unity.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(PropagateFireSystem))]
public partial class SetupGameSystem : SystemBase
{
    [NotBurstCompatible]
    protected override void OnUpdate()
    {
        if (!TryGetSingletonEntity<SpawnMarker>(out Entity marker))
            return;

        EntityManager.DestroyEntity(marker);

        var gameConstants = GetSingleton<GameConstants>();
        int gridSize = gameConstants.FieldSize.x * gameConstants.FieldSize.y;
        
        var random = new Random(12345);

        // TODO: Spawn Lakes
        {
            // TODO: These should be spawned by conversion (remove this comment when that is done)
        }

        // TODO: Spawn Teams of Firefighters
        {
            var fireFighterArray = new NativeArray<Entity>(100, Allocator.Temp);

            EntityManager.Instantiate(gameConstants.FireFighterPrefab, fireFighterArray);

            for (int i = 0; i < fireFighterArray.Length; i++)
            {
                EntityManager.SetComponentData(fireFighterArray[i], new Translation { Value = new float3(random.NextFloat() * gameConstants.FieldSize.x, 0, random.NextFloat() * gameConstants.FieldSize.y) });
                //EntityManager.AddComponentData(fireFighterArray[i], (TargetDestination)(random.NextFloat2() * gameConstants.FieldSize));
                if ((i & 1) == 0)
                {
                    EntityManager.AddComponent<HoldsFullBucket>(fireFighterArray[i]);
                }
                else
                {
                    EntityManager.AddComponent<HoldsEmptyBucket>(fireFighterArray[i]);
                }

            }
        }

        {
            var cellsWithBuckets = (int)(gameConstants.BucketSpawnDensity * gridSize);

            for (int i = 0; i < cellsWithBuckets; i++)
            {
                var bucketEntity = EntityManager.Instantiate(gameConstants.BucketPrefab);
                EntityManager.SetComponentData(bucketEntity, new Translation
                {
                    Value = new float3(random.NextInt(gameConstants.FieldSize.x), 0, random.NextInt(gameConstants.FieldSize.y)) * 0.3f,
                });
                EntityManager.SetComponentData(bucketEntity, new Bucket()
                {
                    Volume = 1//random.NextFloat(1)
                });
            }
        }

        // Fire
        {
            var fireField = EntityManager.CreateEntity(typeof(FireField));
            var heatBuffer = EntityManager.AddBuffer<FireHeat>(fireField);

            // Array of 0's
            var heatArray = new NativeArray<FireHeat>(gridSize, Allocator.Temp, NativeArrayOptions.ClearMemory);

            // Add fire
            var cellsOnFire = (int)(gameConstants.FireSpawnDensity * heatArray.Length);

            for (int i = 0; i < cellsOnFire; i++)
                heatArray[random.NextInt(heatArray.Length)] = random.NextFloat(gameConstants.FireHeatFlashPoint, 1f);

            // Set fire
            heatBuffer.AddRange(heatArray);



            // Show where we spawned fire
            for (int y = 0; y < gameConstants.FieldSize.y; y++)
            {
                for (int x = 0; x < gameConstants.FieldSize.x; x++)
                {
                    if (heatArray[x + y * gameConstants.FieldSize.x] < 0.2f)
                        continue;

                    var flameEntity = EntityManager.Instantiate(gameConstants.FlamePrefab);

                    EntityManager.SetComponentData(flameEntity, new Translation { Value = new float3(x, 0, y) * 0.3f });
                }
            }
            
            // TODO: fix that volume thing
            Entities
                .ForEach((ref OriginalLake originalLake, ref Lake lake, in NonUniformScale scale) =>
                {
                    originalLake.Scale = scale.Value;
                    originalLake.Volume = originalLake.Scale.x * originalLake.Scale.z * gameConstants.LakeMaxVolume;
                    lake.Volume = originalLake.Volume;
                }).ScheduleParallel();
        }
    }
}