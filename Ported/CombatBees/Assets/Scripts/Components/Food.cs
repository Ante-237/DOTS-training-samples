using Unity.Entities;

[GenerateAuthoringComponent]
public struct Food : IComponentData
{
    public bool isBeeingCarried;
}