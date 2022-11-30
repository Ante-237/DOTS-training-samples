using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;

[UpdateAfter(typeof(AntMovementSystem))]
partial struct FoodSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }


    public void OnDestroy(ref SystemState state)
    {
    }


    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<Config>();
        foreach (var foodTransform in SystemAPI.Query<TransformAspect>().WithAll<Food>())
        {
            foreach (var (transform, hasResource) in SystemAPI.Query<TransformAspect, RefRW<HasResource>>().WithAll<Ant>())
            {
                if (math.distance(transform.LocalPosition, foodTransform.LocalPosition) < 1.0f) // TODO Hard coded food radius of 1 m
                    hasResource.ValueRW.Value = true;
            }
        }
    }
}