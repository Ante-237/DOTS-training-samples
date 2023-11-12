using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

public partial struct ObstacleSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Configuration>();
       
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        var configuration = SystemAPI.GetSingleton<Configuration>();

        var rand = new Random(123);
        var scale = configuration.Radius;

        for (int i = 0; i < configuration.NumberOfPickups; i++)
        {
            var obstacle = state.EntityManager.Instantiate(configuration.PickUpPrefab);
            state.EntityManager.SetComponentData(obstacle,
                new LocalTransform
                {
                    Position =  new float3
                    {
                        x = rand.NextFloat(0.1f),
                        y= 0,
                        z = rand.NextFloat(0.5f)
                    },
                    Scale = 0.5f,
                    Rotation = quaternion.identity
                });
        }
        

    }
}