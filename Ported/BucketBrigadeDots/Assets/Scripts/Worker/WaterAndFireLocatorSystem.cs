using Unity.Entities;
using Unity.Mathematics;

public partial struct WaterAndFireLocatorSystem : ISystem
{
    const float k_DefaultGridSize = 0.3f;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameSettings>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space))
        {
            var gameSetting = SystemAPI.GetSingleton<GameSettings>();
            
            var random = Random.CreateFromIndex((uint)SystemAPI.Time.ElapsedTime);
            foreach (var (teamData, teamState) in SystemAPI.Query<
                         RefRW<TeamData>, 
                         RefRW<TeamState>>())
            {
                teamData.ValueRW.FirePosition = random.NextFloat2(float2.zero, gameSetting.RowsAndColumns * k_DefaultGridSize);
                teamData.ValueRW.WaterPosition = random.NextFloat2(float2.zero, gameSetting.RowsAndColumns * k_DefaultGridSize);
                teamState.ValueRW.Value = TeamStates.Idle;
            }
        }
    }
}