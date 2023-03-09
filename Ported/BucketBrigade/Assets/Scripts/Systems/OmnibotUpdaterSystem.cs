using System.Collections;
using System.Collections.Generic;
using Components;
using Enums;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Utilities;

namespace Systems
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(BucketSpawnerSystem))]
    [UpdateAfter(typeof(BotSpawnerSystem))]
    public partial struct OmnibotUpdaterSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ConfigAuthoring.Config>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<ConfigAuthoring.Config>();
            var heatMap = SystemAPI.GetSingletonBuffer<ConfigAuthoring.FlameHeat>();

            foreach (var (botTransform, command, botEntity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<BotCommand>>()
                     .WithAll<BotOmni>()
                     .WithEntityAccess())
            {
                var targetBucket = state.EntityManager.GetComponentData<TargetBucket>(botEntity);
                var targetFire = state.EntityManager.GetComponentData<TargetFlame>(botEntity);

                switch (command.ValueRW.Value)
                {
                    case BotAction.GET_BUCKET:
                        {
                            if (targetBucket.value == Entity.Null)
                            {
                                Debug.Log($"Finding bucket");
                                targetBucket.value = FindBucket(ref state, in botTransform.ValueRO.Position);
                                state.EntityManager.SetComponentData(botEntity, new TargetBucket { value = targetBucket.value });
                            }
                            else
                            {
                                LocalTransform bucketTransform = state.EntityManager.GetComponentData<LocalTransform>(targetBucket.value);

                                if (Utils.MoveTowards(ref botTransform.ValueRW, bucketTransform.Position, config.botSpeed, config.botArriveThreshold))
                                {
                                    state.EntityManager.SetComponentData(targetBucket.value, new Bucket { isActive = true, isFull = false });
                                    command.ValueRW.Value = BotAction.GOTO_WATER;
                                }
                            }
                            break;
                        }
                    case BotAction.GOTO_WATER:
                        {
                            var targetWater = state.EntityManager.GetComponentData<TargetWater>(botEntity);
                            if (targetWater.value == Entity.Null)
                            {
                                Debug.Log($"Finding water");
                                targetWater.value = FindWater(ref state, in botTransform.ValueRO.Position);
                                state.EntityManager.SetComponentData(botEntity, new TargetWater { value = targetWater.value });
                            }
                            else
                            {
                                WorldTransform waterTransform = state.EntityManager.GetComponentData<WorldTransform>(targetWater.value);

                                if (Utils.MoveTowards(ref botTransform.ValueRW, waterTransform.Position, config.botSpeed, config.botArriveThreshold))
                                {
                                    command.ValueRW.Value = BotAction.FILL_BUCKET;
                                }
                            }
                            break;
                        }
                    case BotAction.FILL_BUCKET:
                        {
                            var bucketVolume = state.EntityManager.GetComponentData<Volume>(targetBucket.value);
                            bucketVolume.value = Mathf.Clamp(bucketVolume.value + config.bucketFillRate, 0f, config.bucketCapacity);
                            state.EntityManager.SetComponentData(targetBucket.value, new Volume { value = bucketVolume.value });

                            var targetWater = state.EntityManager.GetComponentData<TargetWater>(botEntity);
                            var waterVolume = state.EntityManager.GetComponentData<Volume>(targetWater.value);

                            waterVolume.value -= config.bucketFillRate;
                            state.EntityManager.SetComponentData(targetWater.value, new Volume { value = waterVolume.value });

                            if (bucketVolume.value >= config.bucketCapacity)
                            {
                                state.EntityManager.SetComponentData(targetBucket.value, new Bucket { isActive = true, isFull = true });
                                state.EntityManager.SetComponentData(targetBucket.value, new URPMaterialPropertyBaseColor() { Value = config.bucketFullColor });
                                state.EntityManager.SetComponentData(botEntity, new TargetWater { value = Entity.Null });
                                command.ValueRW.Value = BotAction.GOTO_FIRE;
                            }

                            //// Update bucket Scale (reparenting trick to preserve scale)
                            //carrying.transform.SetParent(fireSim.transform, true);
                            //carrying.UpdateBucket();
                            //carrying.transform.SetParent(t, true);

                            break;
                        }
                    case BotAction.GOTO_FIRE:
                        {
                            if (targetFire.value == Entity.Null)
                            {
                                Debug.Log($"Finding fire");
                                var foundFire = FindFire(ref state, in botTransform.ValueRO.Position);
                                state.EntityManager.SetComponentData(botEntity, new TargetFlame { value = foundFire });
                            }
                            else
                            {
                                LocalTransform fireTransform = state.EntityManager.GetComponentData<LocalTransform>(targetFire.value);
                                if (Utils.MoveTowards(ref botTransform.ValueRW, fireTransform.Position, config.botSpeed, config.botArriveThreshold))
                                {
                                    command.ValueRW.Value = BotAction.THROW_BUCKET;
                                }
                            }
                            break;
                        }
                    case BotAction.THROW_BUCKET:
                        {
                            var flameCell = state.EntityManager.GetComponentData<FlameCell>(targetFire.value);
                            heatMap[flameCell.heatMapIndex] = new ConfigAuthoring.FlameHeat { Value = heatMap[flameCell.heatMapIndex].Value - config.coolingStrength };

                            if (heatMap[flameCell.heatMapIndex].Value < config.flashpoint)
                                state.EntityManager.SetComponentData(botEntity, new TargetFlame { value = Entity.Null });

                            state.EntityManager.SetComponentData(targetBucket.value, new Bucket { isActive = true, isFull = false });
                            state.EntityManager.SetComponentData(targetBucket.value, new Volume { value = 0 });
                            state.EntityManager.SetComponentData(targetBucket.value, new URPMaterialPropertyBaseColor() { Value = config.bucketEmptyColor });

                            command.ValueRW.Value = BotAction.GOTO_WATER;
                            break;
                        }
                }
            }
        }


        public Entity FindBucket(ref SystemState state, in float3 botPos, bool wantsFull = false)
        {
            var minDistance = float.PositiveInfinity;
            var closestBucket = Entity.Null;

            foreach (var (bucketTransform, bucket, bucketEntity)
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Bucket>>()
                         .WithEntityAccess())
            {
                if (bucket.ValueRO.isActive == true) continue;

                var distance = math.distancesq(botPos, bucketTransform.ValueRO.Position);

                if (distance < minDistance)
                {
                    closestBucket = bucketEntity;
                    minDistance = distance;
                }
            }

            return closestBucket;
        }

        public Entity FindWater(ref SystemState state, in float3 botPos)
        {
            var minDistance = float.PositiveInfinity;
            var closestWater = Entity.Null;

            foreach (var (waterTransform, waterEntity)
                     in SystemAPI.Query<RefRO<WorldTransform>>()
                         .WithAll<WaterAuthoring.Water>()
                         .WithEntityAccess())
            {
                var distance = math.distancesq(botPos, waterTransform.ValueRO.Position);

                if (distance < minDistance)
                {
                    closestWater = waterEntity;
                    minDistance = distance;
                }
            }

            return closestWater;
        }

        public Entity FindFire(ref SystemState state, in float3 botPos)
        {
            var minDistance = float.PositiveInfinity;
            var closestFire = Entity.Null;

            foreach (var (fireTransform, flameCell, fireEntity)
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRO<FlameCell>>()
                         .WithEntityAccess())
            {
                if (flameCell.ValueRO.isOnFire == false) continue;

                var distance = math.distancesq(botPos, fireTransform.ValueRO.Position);

                if (distance < minDistance)
                {
                    closestFire = fireEntity;
                    minDistance = distance;
                }
            }

            if (closestFire != null) Debug.Log("Fire Found");

            return closestFire;
        }
    }
}