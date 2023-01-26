using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
partial struct PlantGrowthSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldGrid>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float elapsedTime = (float)SystemAPI.Time.ElapsedTime;
        float timeSincePlanted;

        //UnityEngine.Debug.Log("Trying to grow a plant");
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var worldGrid = SystemAPI.GetSingleton<WorldGrid>();

        foreach (var plant in SystemAPI.Query<PlantAspect>().WithNone<PlantFinishedGrowing>())
        {
            //UnityEngine.Debug.Log("PREgrowing a plant " + plant.ReadyToPick + " and " + plant.PickedAndHeld);
            if (plant.ReadyToPick || plant.PickedAndHeld)
            {
                continue;
            }

            //plant is not fully grown

            timeSincePlanted = elapsedTime - plant.TimePlanted;
            float scale = timeSincePlanted / plant.Plant.ValueRW.timeToGrow;
            scale = math.clamp(scale, 0, 1);
            plant.Transform.LocalScale = scale;

            if(scale >= 1) //plant is now grown
            {
                //state.EntityManager.AddComponent<PlantFinishedGrowing>(plant.Self);
                ecb.AddComponent<PlantFinishedGrowing>(plant.Self);
                plant.ReadyToPick = true;
                var plotAspect = SystemAPI.GetAspectRO<PlotAspect>(plant.Plot);
                worldGrid.SetTypeAt(plotAspect.PlotLocInWorld, PlantFinishedGrowing.type);
                worldGrid.SetEntityAt(plotAspect.PlotLocInWorld, plant.Self);

            }
        }
    }
}