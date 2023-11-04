using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

public class UnderstandingScript : MonoBehaviour
{
    public Matrix4x4 M0;
    public float value = 4;
    private float initialSmooth = 0.0f;
    public RectInt Rectstuff;
    
    public Material material;
    public Mesh mesh;
    const int numInstances = 10;
    public float movementValue = 10;

    struct MyInstanceData
    {
        public Matrix4x4 objectToWorld;
        public float myOtherData;
        public uint renderingLayerMask;
    };
    
    private void Update()
    {
        RenderParams rp = new RenderParams(material);
        MyInstanceData[] instData = new MyInstanceData[numInstances];
        for(int i=0; i<numInstances; ++i)
        {
            instData[i].objectToWorld = Matrix4x4.Translate(new Vector3(i, 0.0f, 5.0f + (i * 0.5f))) * Matrix4x4.Scale(new Vector3(0.1f, 2f, 0.1f));
            instData[i].renderingLayerMask = (i & 1) == 0 ? 1u : 2u;
        }

        for (int i = 0; i < instData.Length; i++)
        {
            instData[i].objectToWorld.m23 = ( movementValue * Time.deltaTime);
        }
        
        Graphics.RenderMeshInstanced(rp, mesh, 0, instData);
    }
    
    private void Start()
    {
        // initialSmooth = Mathf.Pow(value, Time.deltaTime);
    }
     //private void Update()
     // {
        // Graphics.DrawMeshInstanced(Player, 0, PlayerMaterial, M0);
     // }


    private void RunTimerDelta()
    {
        float smooth = Mathf.Pow(value, Time.deltaTime);
        if (smooth > initialSmooth)
        {
            Debug.LogWarning("Factor is Increasing");
            initialSmooth = smooth;
        }
        else if (smooth < initialSmooth)
        {
            Debug.LogWarning("Factor is Decreasing");
            initialSmooth = smooth;
        }

        // Debug.LogWarning($"The Smoothing is :::  {smooth.ToString()}");
        Debug.LogWarning($"Time.deltaTime {Time.deltaTime.ToString()}");
    }
}
