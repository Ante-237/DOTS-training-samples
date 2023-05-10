using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial struct FireSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<Fire>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var config = SystemAPI.GetSingleton<Grid>();
        var targetQuery = SystemAPI.QueryBuilder().WithAll<Fire>().Build();

        var job = new FireJob {
            neighoringFires = targetQuery.ToComponentDataArray<Fire>(state.WorldUpdateAllocator),
            rate = SystemAPI.Time.DeltaTime * config.FireGrowthRate,
            spreadVal = config.FireSpreadValue,
            startingColor = config.StartingGridColor,
            fullBurnColor = config.FullBurningGridColor,
            gridSize =  config.GridSize
        };
        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[WithAll(typeof(Fire))]
[BurstCompile]
public partial struct FireJob : IJobEntity {
    [ReadOnly] public NativeArray<Fire> neighoringFires;
    public float4 startingColor;
    public float4 fullBurnColor;
    public float rate;
    public float spreadVal;
    public int gridSize;

    void Execute([EntityIndexInQuery] int index, ref Fire fire, ref URPMaterialPropertyBaseColor color, ref LocalTransform transform) {
        if (fire.t > 0.0f) {
            fire.t = math.clamp(fire.t + rate, 0.0f, 1.0f);
		} else
        {
            int leftNeighborIndex = index - 1;
            int rightNeighborIndex = index + 1;
            int upNeighborIndex = index + gridSize;
            int downNeighborIndex = index - gridSize;
            
            // Look at all neighbors for flashpoint.  Watch for boundaries on grid
            bool flashpoint = false;
            if (leftNeighborIndex >= 0 && neighoringFires[leftNeighborIndex].t > spreadVal)
                flashpoint = true;
            else if (rightNeighborIndex < neighoringFires.Length && neighoringFires[rightNeighborIndex].t > spreadVal)
                flashpoint = true;
            else if (upNeighborIndex < neighoringFires.Length&& neighoringFires[upNeighborIndex].t > spreadVal)
                flashpoint = true;
            else if (downNeighborIndex >= 0 && neighoringFires[downNeighborIndex].t > spreadVal)
                flashpoint = true;
            
            if (flashpoint) {
                fire.t = Random.CreateFromIndex((uint)index).NextFloat(math.EPSILON, 0.15f);
            }
        }

        transform.Position.y = math.lerp(-2.0f, 2.0f, fire.t);
        color.Value = math.lerp(startingColor, fullBurnColor, fire.t);

        // Over the flashpoint and we add a sine anim
    }
}