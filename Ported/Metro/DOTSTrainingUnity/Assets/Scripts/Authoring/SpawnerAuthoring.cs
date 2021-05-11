using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
    , IConvertGameObjectToEntity
    , IDeclareReferencedPrefabs
{
    public GameObject LanePrefab;
    [Range(0, 1000)] public int LaneCount;

    public GameObject CarPrefab;
    [Range(0, 1)] public float CarFrequency;

    // This function is required by IDeclareReferencedPrefabs
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        // Conversion only converts the GameObjects in the scene.
        // This function allows us to inject extra GameObjects,
        // in this case prefabs that live in the assets folder.
        referencedPrefabs.Add(LanePrefab);
        referencedPrefabs.Add(CarPrefab);
    }

    // This function is required by IConvertGameObjectToEntity
    public void Convert(Entity entity, EntityManager dstManager
        , GameObjectConversionSystem conversionSystem)
    {
        // GetPrimaryEntity fetches the entity that resulted from the conversion of
        // the given GameObject, but of course this GameObject needs to be part of
        // the conversion, that's why DeclareReferencedPrefabs is important here.
        dstManager.AddComponentData(entity, new Spawner
        {
            LanePrefab = conversionSystem.GetPrimaryEntity(LanePrefab),
            LaneCount = LaneCount,
            CarPrefab = conversionSystem.GetPrimaryEntity(CarPrefab),
            CarFrequency = CarFrequency
        });
    }
}