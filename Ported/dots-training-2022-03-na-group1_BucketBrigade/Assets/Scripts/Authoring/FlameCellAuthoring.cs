﻿using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityMonoBehaviour = UnityEngine.MonoBehaviour;
using UnityMeshRenderer = UnityEngine.MeshRenderer;

public class FlameCellAuthoring : UnityMonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager
        , GameObjectConversionSystem conversionSystem)
    {
        //var allRenderers = transform.GetComponentsInChildren<UnityMeshRenderer>();
        //var flameEntity = new NativeArray<Entity>(allRenderers.Length, Allocator.Temp);

        // for(int i = 0; i < allRenderers.Length; ++i)
        // {
        //     var meshRenderer = allRenderers[i];
        //     flameEntity[i] = conversionSystem.GetPrimaryEntity(meshRenderer.gameObject);
        // }
        

        dstManager.RemoveComponent<Unity.Transforms.Rotation>(entity);
        dstManager.AddComponent<Unity.Transforms.NonUniformScale>(entity);

        // We could have used AddComponent in the loop above, but as a general rule in
        // DOTS, doing a batch of things at once is more efficient.
        dstManager.AddComponent<URPMaterialPropertyBaseColor>(entity);
        dstManager.AddComponent<Position>(entity);
        dstManager.AddComponent<Color>(entity);
        dstManager.AddComponent<Scale>(entity);
        dstManager.AddComponent<FireIndex>(entity);
    }
}
