﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class OmnibotAuthoring : MonoBehaviour
{
    class Baker : Baker<OmnibotAuthoring>
    {
        public override void Bake(OmnibotAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Omnibot
            {
                OmnibotState = OmnibotState.LookForWater, 
                t = 0,
                TravelSpeed = 5f,
                WaterGatherSpeed = .1f,
                MaxWaterCapacity = .3f,
                MaxDouseAmount = 1.5f,
                DouseRadius = 6
            });
            
        }
    }
}

public enum OmnibotState
{
    LookForWater,
    TravelToWater,
    GatherWater,
    LookForFire,
    TravelToFire,
    DouseFire
}

public struct Omnibot : IComponentData
{
    public float t;
    
    public float3 TargetPos;
    public Entity TargetWaterEntity;
    public Entity TargetFireEntity;

    public OmnibotState OmnibotState;
    public float TravelSpeed;
    
    public float MaxWaterCapacity;
    public float WaterGatherSpeed;
    public float CurrentWaterCarryingVolume;

    public float DouseRadius;
    public float MaxDouseAmount;
}