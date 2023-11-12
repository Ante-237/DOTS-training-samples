using System;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class MathTest : MonoBehaviour
{
    float2x2  _position = float2x2.zero;
    float4x4 _matrixPosition = float4x4.identity;
   

    private float3x2 _matrixthreebyTwo = float3x2.zero;
    private int3 _vectorThree = new int3(1, 2, 4);
    private int4 _vectorFour = new int4(1, 2, 3, 4);

    [Header("CHANGE STATE")] 
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float x_grow;

    [SerializeField] 
    [Range(0.0f, 10.0f)] 
    private float y_grow;

    [SerializeField] 
    [Range(0.0f, 10.0f)] 
    private float z_grow;
    
    // matrix manipulations. 
    int2x3 m = new int2x3(1, 2, 3, 4, 5, 6);// first row: 1, 2, 3
    // second row: 4, 5, 6

    private void matrixCheck()
    {
        int2 i2 = m.c2;
        // build from access columns
        int2 i0 = m.c0.xy;
        Debug.LogError($"Access of Matrix third column {i2.ToString()}");
    }
    

    private void LowerCheck()
    {
        _matrixthreebyTwo = _vectorThree[0];
        _vectorFour = _vectorThree.xyxx;
        
    }

    private void RandomCheck()
    {
        Random rand = new Random(123);
   
    }
    
    private void Start()
    {
        Debug.Log($"{_position.ToString()}");
        Debug.Log($"{_matrixPosition.ToString()}");
        // matrixCheck();
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(new float3(1, 1, 1), new float3(x_grow, y_grow, z_grow));
        
    }
}
