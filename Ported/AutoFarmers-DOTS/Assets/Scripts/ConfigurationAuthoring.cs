using Unity.Entities;
using UnityEngine;

public class ConfigurationAuthoring : MonoBehaviour
{

    public int NumberOfPickups;
    public int NumberOfDrops;
    public int NumberOfWorkers;
    [Range(0.0f, 10f)] public float DistanceApart = 0.2f;
    [Range(0.0f, 100f)] public float speedMove = 1.0f;

    public GameObject WorkerMan;
    public GameObject PickUps;
    public GameObject Drops;
    
    private class ConfigurationBaker : Baker<ConfigurationAuthoring>
    {
        public override void Bake(ConfigurationAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Configuration
            {
                NumberOfPickups =  authoring.NumberOfPickups,
                NumberOfDrops =  authoring.NumberOfDrops,
                NumberOfWorkers = authoring.NumberOfWorkers,
                Speed =  authoring.speedMove,
                Radius = authoring.DistanceApart,
                
                
                WorkerManPrefab = GetEntity(authoring.WorkerMan, TransformUsageFlags.Dynamic),
                PickUpPrefab = GetEntity(authoring.PickUps, TransformUsageFlags.Dynamic),
                DropPrefab = GetEntity(authoring.Drops, TransformUsageFlags.Dynamic)
            });
        }
    }
}


public struct Configuration : IComponentData
{
    public int NumberOfPickups;
    public int NumberOfDrops;
    public int NumberOfWorkers;
    public float Speed;
    public float Radius;
    public Entity WorkerManPrefab;
    public Entity PickUpPrefab;
    public Entity DropPrefab;
}
