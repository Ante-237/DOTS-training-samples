﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    public class SowField_FindCellSystem : SystemBase
    {
        private EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            _entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

            GetEntityQuery(ComponentType.ReadOnly<CellEntityElement>());
            GetEntityQuery(ComponentType.ReadOnly<CellTypeElement>());
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer.Concurrent ecb = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
            int2 GridSize = GetSingleton<Grid>().Size;

            Entity grid = GetSingletonEntity<Grid>();
            Grid gridComponent = EntityManager.GetComponentData<Grid>(grid);
            int2 gridSize = gridComponent.Size;
            DynamicBuffer<CellTypeElement> typeBuffer = EntityManager.GetBuffer<CellTypeElement>(grid);
            DynamicBuffer<CellEntityElement> cellEntityBuffer = EntityManager.GetBuffer<CellEntityElement>(grid);

            Entities
                .WithAll<PlantSeeds_Intent>()
                .WithNone<TargetReached>()
                .WithNone<PathFindingTarget>()
                .WithReadOnly(typeBuffer)
                .WithReadOnly(cellEntityBuffer)
                .ForEach((int entityInQueryIndex, Entity entity, ref RandomSeed randomSeed, in Translation translation) =>
                {
                    int2 plantPos = new int2(0, 0);                    

                    bool suitablePosFound = false;
                    // Find a suitable area to sow on tilled land
                    {
                        int2 position = CellPosition.FromTranslation(translation.Value);
                        Random random = new Random(randomSeed.Value);
                        int index = 0;

                        int windowSize = 16;
                        int maxTries = 20;
                        int numTries = maxTries;

                        // Random
                        while (!suitablePosFound && numTries > 0)
                        {
                            numTries--;

                            if (numTries < maxTries / 2)
                            {
                                plantPos = position + random.NextInt2(new int2(0, 0), windowSize) + new int2(-windowSize / 2, -windowSize / 2);
                                plantPos = math.clamp(plantPos, new int2(0, 0), GridSize - new int2(1, 1));
                            }
                            else
                            {
                                plantPos = random.NextInt2(new int2(0, 0), GridSize - 1);
                            }
                                                        
                            randomSeed.Value = random.state;

                            index = gridComponent.GetIndexFromCoords(plantPos.x, plantPos.y);
                            if (index < 0 || index >= GridSize.x * GridSize.y)
                            {
                                UnityEngine.Debug.LogError("FindSowField went out-of-bounds");
                                suitablePosFound = false;
                                break;
                            }

                            if (typeBuffer[index].Value == CellType.Tilled)
                            { 
                                suitablePosFound = true;
                            }
                        }
                    }

                    if(suitablePosFound)
                    {
                        ecb.RemoveComponent<PathFindingTargetReached_Tag>(entityInQueryIndex, entity);

                        int index = gridComponent.GetIndexFromCoords(plantPos.x, plantPos.y);
                        Entity targetEntity = cellEntityBuffer[index].Value;

                        ecb.AddComponent(entityInQueryIndex, entity, new PathFindingTarget()
                        {
                            Value = targetEntity
                        });
                    }
                    else
                    {
                        ecb.RemoveComponent<PlantSeeds_Intent>(entityInQueryIndex, entity);
                    }
                }).ScheduleParallel();

            _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
