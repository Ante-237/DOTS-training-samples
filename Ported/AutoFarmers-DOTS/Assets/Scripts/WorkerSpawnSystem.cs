
using System;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;


public partial struct WorkSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Configuration>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var configuration = SystemAPI.GetSingleton<Configuration>();
        var rand = new Random(392);

        for (int i = 0; i < configuration.NumberOfWorkers; i++)
        {
            var worker = state.EntityManager.Instantiate(configuration.WorkerManPrefab);
            
            state.EntityManager.SetComponentData(worker, new LocalTransform
            {
                Position =
                {
                    x = rand.NextFloat() * 20f,
                    y = 0f,
                    z = rand.NextFloat() * 20f
                },
                Rotation = Quaternion.identity,
                Scale =  0.5f
            });
            
            state.EntityManager.AddComponent<Worker>(worker);
            

        }
    }

    public void OnDestroy(ref SystemState state)
    {
        
    }
}