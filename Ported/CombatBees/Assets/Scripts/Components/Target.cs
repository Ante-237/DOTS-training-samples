using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Target : IComponentData
{
    public Entity Value;
    public float3 TargetPosition;
}

