using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Object = System.Object;


public class UnderstandingScript : MonoBehaviour
{
    /*
    public Matrix4x4 M0;
    public float value = 4;
    private float initialSmooth = 0.0f;
    public RectInt Rectstuff;
    
    public Material material;
    public Mesh mesh;
    const int numInstances = 10;
    public float movementValue = 10;
    */
    
    // testing dot product analogy
    public Transform BoxOne;
    public Transform BoxTwo;
    public Transform ObjectToRotate;
    [Range(0.0f, 100f)] public float value = 0.4f;

    private float moveValue = 0.0f;
    public Vector3 SumVector;
    public Vector3 SubVector;
    public Vector3 CrossProduct;
    public float EndPosition;

    public bool MoveRight = false;
    public float SineValue = 0;
    public float CosValue = 0;
    public float Angle = 0;
    public float speed = 5.0f;


    public Matrix4x4 MeshMatrix;
    private Matrix4x4 ResourceMatrix;
    public Mesh ResourceMesh;
    public Mesh Plane;
    public Material _material;
    MaterialPropertyBlock MaterialProperty;
    public Texture2D _texture2D;
    private Vector4 _color;


    [Space(20)] [Header("Testing Rotation")]
    public GameObject RotateObject;

    public Vector3 NewRotation;
    private readonly float pi_range_low = 0.1f;
    private readonly float pi_range_high = 2f;

    private void PlayingWithMaterial()
    {
        MaterialProperty = new MaterialPropertyBlock();
        _color = new Vector4(0, 0, 1, 1);
        MaterialProperty.SetColor(0, _color);
        MeshMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0f),Quaternion.identity, new Vector3( 0.05f, 0.02f, 0.08f) );
        ResourceMatrix = Matrix4x4.TRS(new Vector3(0, 0.01f, 0), Quaternion.identity, new Vector3(0.01f, 0.01f, 0.01f));
        
    }

    private void PlayingWithRotation()
    {
        float x = RotateObject.transform.position.x;
        float y = RotateObject.transform.position.z;
        

        float _x =( x * Mathf.Cos(Mathf.PI )) - (y * (Mathf.Sin(Mathf.PI )));
        float _y =( y * Mathf.Sin(Mathf.PI )) + (y * (Mathf.Cos(Mathf.PI )));
        NewRotation = new Vector3(_x, 0, _y);

        RotateObject.transform.position = NewRotation;
    }

    private void Start()
    {
        PlayingWithMaterial();
        PlayingWithRotation();
    }

    private void PlayingWithMesh()
    {
        
    }

    private void Update()
    {
        moveValue = Mathf.PingPong(Time.time, 2f);
        BoxTwo.transform.position = new Vector3(CosValue * 3, moveValue, SineValue * 3);
        //PlayingWithRotation();
        //CalculatingDotProduct();
        //CalculateAddition();
        //CalculateSubtraction();
        //CalculateCrossProduct();
        CalculateSinFunction();
        CalculateCosFunction();
        CalculateMove();
        CalculateAngle3D();
        CalculateDotProductCos();
        // CalculateAngle();
        MeshMatrix = Matrix4x4.TRS(new Vector3(0, 0, moveValue), Quaternion.identity, new Vector3(0.05f, 0.02f, 0.08f));

        Graphics.DrawMesh(ResourceMesh, MeshMatrix, _material, 0);
        Graphics.DrawMesh(Plane, ResourceMatrix, _material, 0);

        Bounds n = ResourceMesh.bounds;

        //  if (MoveRight)
        // {
        //     BoxTwo.transform.position = new Vector3(moveValue, BoxTwo.transform.position.y, BoxTwo.transform.position.z);
        // }
    }

    void CalculatingDotProduct()
    {
        float deathpoint = Vector3.Dot(BoxOne.transform.position, BoxTwo.transform.position);
        float additionDeadpoint = Vector3.Dot(BoxOne.transform.position, SumVector);
        float subDeadpoint = Vector3.Dot(BoxOne.transform.position, SubVector);
        
        if (deathpoint > 0 )
        {
            Debug.DrawLine(BoxOne.transform.position, BoxTwo.transform.position, Color.white);
        }

        if (additionDeadpoint > 0)
        {
            Debug.DrawLine(BoxOne.transform.position, SumVector, Color.white);
        }

        if (subDeadpoint > 0)
        {
            Debug.DrawLine(BoxOne.transform.position, SubVector, Color.white);
        }
        
        if (deathpoint < 0 || additionDeadpoint < 0 || subDeadpoint < 0)
        {
            Debug.DrawLine(BoxOne.transform.position, BoxTwo.transform.position, Color.red);
        }

        if (additionDeadpoint < 0)
        {
            Debug.DrawLine(BoxOne.transform.position, SumVector, Color.red);
        }

        if (subDeadpoint < 0)
        {
            Debug.DrawLine(BoxOne.transform.position, SubVector, Color.red);
        }

        
        
        if (deathpoint == 0 || additionDeadpoint == 0 || subDeadpoint == 0)
        {
            Debug.DrawLine(BoxOne.transform.position, BoxTwo.transform.position, Color.yellow);
            Debug.DrawLine(BoxOne.transform.position, SumVector, Color.yellow);
            Debug.DrawLine(BoxOne.transform.position, SubVector, Color.yellow);
        }
        
        
    }

    void CalculateAddition()
    {
        // will display a red box
        SumVector = BoxOne.transform.position + BoxTwo.transform.position;
        
    }

    void CalculateSubtraction()
    {
        // will display a magneta color
        SubVector = BoxOne.transform.position - BoxTwo.transform.position;
    }

    void CalculateCrossProduct()
    {
        // will display in green
        CrossProduct = Vector3.Cross(BoxOne.transform.position, BoxTwo.transform.position);
    }

    void CalculateSinFunction()
    {
        SineValue = Mathf.Sin(Time.time) * 0.25f;
    }

    void CalculateCosFunction()
    {
        CosValue = Mathf.Cos(Time.time) * 0.25f;
    }

    void CalculateMove()
    {
        EndPosition = Mathf.Lerp(BoxOne.transform.position.z, BoxTwo.transform.position.z, Time.time);
    }

    void CalculateAngle()
    {
        Vector3 difference = ObjectToRotate.transform.position - BoxTwo.transform.position;
        Angle = Mathf.Atan2(difference.z, difference.x) * Mathf.Rad2Deg;
        Angle = Mathf.LerpAngle(ObjectToRotate.transform.eulerAngles.z, Angle, Time.time);
        ObjectToRotate.rotation = Quaternion.Euler(ObjectToRotate.transform.eulerAngles.x, -Angle, ObjectToRotate.transform.eulerAngles.z);
        Debug.DrawLine(ObjectToRotate.transform.position, BoxTwo.transform.position, Color.red);
    }

    void CalculateAngle3D()
    {
        Vector3 targetDirection = ObjectToRotate.transform.position - BoxTwo.transform.position;

        // The step size is equal to speed times frame time.
        float singleStep = speed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        //Vector3 newDirection = Vector3.RotateTowards(ObjectToRotate.transform.forward, targetDirection, singleStep, 0.0f);

        // Draw a ray pointing at our target in
        Debug.DrawRay(ObjectToRotate.transform.position, -targetDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        ObjectToRotate.transform.rotation = Quaternion.LookRotation(-targetDirection);
    }

    void CalculateDotProductCos()
    {
        float dotValue = (Vector3.Dot(ObjectToRotate.transform.position, BoxOne.transform.position));
        float length = Vector3.Magnitude(ObjectToRotate.transform.position) * Vector3.Magnitude(BoxOne.transform.position);

        float valueToCompute = dotValue / length;
        
        Debug.LogWarning($" Value Angle to Compute : {valueToCompute.ToString()}");
        Angle = Mathf.Acos(valueToCompute) * Mathf.Rad2Deg;
    }
    
    
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position , BoxOne.transform.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(value, value , value));
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(BoxTwo.transform.position, new Vector3(value, value, value));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(BoxOne.transform.position, new Vector3(value, value, value));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(BoxOne.transform.position.x, BoxOne.transform.position.y, EndPosition), new Vector3(value, value, value));


        //Gizmos.color = Color.cyan;
        //Gizmos.DrawWireCube(new Vector3(CosValue, SineValue, transform.position.z), new Vector3(value,value, value));

        // for addition box. 
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireCube(SumVector, new Vector3(value, value, value));

        // Gizmos.color = Color.magenta;
        // Gizmos.DrawWireCube(SubVector, new Vector3(value, value, value));

        //Gizmos.color = Color.green;
        //Gizmos.DrawWireCube(CrossProduct, new Vector3(value, value, value));

    }


    /*
    struct MyInstanceData
    {
        public Matrix4x4 objectToWorld;
        public float myOtherData;
        public uint renderingLayerMask;
    };
    */
    
    /*
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
    */
    
    /*
    private void Start()
    {
        // initialSmooth = Mathf.Pow(value, Time.deltaTime);
    }
    */
    //private void Update()
     // {
        // Graphics.DrawMeshInstanced(Player, 0, PlayerMaterial, M0);
     // }
     
    /*
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
    */
}
