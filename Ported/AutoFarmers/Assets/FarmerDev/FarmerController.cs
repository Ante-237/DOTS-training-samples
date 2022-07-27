using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct FarmerController : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (farmer,speed) in  SystemAPI.Query<TransformAspect, RefRO<FarmerSpeed>>().WithAll<Farmer>())
        {
            farmer.Position += new float3(1, 0, 0)*state.Time.DeltaTime;
        }

      //  FarmerJob job = new FarmerJob{deltaTime = state.Time.DeltaTime};
        //job.ScheduleParallel();
    }
}

public partial struct FarmerJob : IJobEntity
{
    // public float deltaTime;
    // public void Execute(TransformAspect farmer, RefRO<FarmerSpeed> speed)
    // {
    //     if (speed.ValueRO.MovementSpeed > 5f)
    //     {
    //         farmer.Position += new float3(1, 0, 0)*deltaTime;
    //     }
    // }
}
