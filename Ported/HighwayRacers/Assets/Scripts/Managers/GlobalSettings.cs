using Unity.Entities;

public struct GlobalSettings : IComponentData
{
    public Entity carPrefab;    
    
    public int amount;

    public bool spawned;
    
    public int NumLanes;
    
    public float LengthLanes;

    public float MinVelocity;
    public float MaxVelocity;

    public float MinOvertakeVelocity;
    public float MaxOvertakeVelocity;
}
