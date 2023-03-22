using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

[BurstCompile]
public partial struct GridTilesSpawningSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Config>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Let OnUpdate run for one frame
        state.Enabled = false;

        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var config = SystemAPI.GetSingleton<Config>();

        for (int i = 0; i <= config.columns; i++)
        {
            for (int j = 0; j <= config.rows; j++)
            {
                var groundTile = ecb.Instantiate(config.Ground);
                
                ecb.SetComponent(groundTile, new LocalTransform
                {
                    Position = new float3
                    {
                        x = i * config.cellSize,
                        y = - (config.maxFlameHeight * 0.5f),
                        z = j * config.cellSize
                    },
                    Scale = 1f,
                    Rotation = quaternion.identity
                });              

                //Make sure all tiles are not on fire by default
                ecb.SetComponentEnabled<OnFire>(groundTile,false);
                ecb.SetComponent(groundTile, new URPMaterialPropertyBaseColor { Value = (UnityEngine.Vector4)config.colour_fireCell_neutral });

                ecb.AddComponent(groundTile, new PostTransformScale { Value = float3x3.Scale(config.cellSize, config.maxFlameHeight, config.cellSize) });
            }
        }
    }
}
