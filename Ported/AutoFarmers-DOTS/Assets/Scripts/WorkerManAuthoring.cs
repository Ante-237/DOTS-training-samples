using Unity.Entities;
using UnityEngine;

public class WorkerManAuthoring : MonoBehaviour
{
    
    private class WorkerManBaker : Baker<WorkerManAuthoring>
    {
        public override void Bake(WorkerManAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<Worker>(entity);
        }
    }
    
}



public struct Worker : IComponentData
{
    
}

public struct PickUp : IComponentData
{
    
}

public struct Drop : IComponentData
{
    
}