using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Unity.Mathematics;
using UnityEditor.Overlays;
using Vector3 = UnityEngine.Vector3;

public class WorkerMan : MonoBehaviour
{
    [SerializeField] private bool isCollected = false;
    [SerializeField] [Range(1, 100f)] public float WorkerSpeed = 10;
    [SerializeField] [Range(1, 100f)] private float DistanceApart = 1;

    public Transform PickupPosition;
    public Transform DropPosition;


    private MeshRenderer _workerRenderer;
    private GameObject _foodCarry;
    private void Start()
    {
        _workerRenderer = GetComponent<MeshRenderer>();
        _foodCarry = transform.GetChild(1).gameObject;
    }

    public void Update()
    {
        WorkerUpdate();
    }

    private void WorkerUpdate()
    {
        
        if (isCollected)
        {
            if (CalculateDistanceApart(DropPosition.position))
            {
                isCollected = !isCollected;
                float _colorAlpha = Mathf.Lerp(0, 1, Time.deltaTime * 10000);
                _workerRenderer.material.color = new Color(0f, 0.5f, 0.5f, _colorAlpha);
                _foodCarry.SetActive(true);

            }
          

            ChangeMovements(DropPosition.position);
        }
        else if (!isCollected)
        {
            
            if (CalculateDistanceApart(PickupPosition.position))
            {
                isCollected = !isCollected;
                float _colorAlpha = Mathf.Lerp(0, 1, Time.deltaTime * 1000);
                _workerRenderer.material.color = new Color(1, 0, 0, _colorAlpha);
                _foodCarry.SetActive(false);
               
            }
           
            ChangeMovements((PickupPosition.position));
        }
        
        
    }

    private bool CalculateDistanceApart(float3 DifferenceApart)
    {
        float distance = Vector3.Distance(transform.position, DifferenceApart);
        if (distance <= DistanceApart)
        {
            return true;
        }
        
        return false;
    }


    private void ChangeMovements(float3 DirectionWorker)
    {
        transform.position = Vector3.MoveTowards(transform.position, DirectionWorker, Time.deltaTime * WorkerSpeed);
        OrientatePosition(DirectionWorker);
    }

    private void OrientatePosition(Vector3 _direction)
    {
        /*
        Vector3 realDirection = (_direction - transform.position).normalized;
        float _angle = Mathf.Atan2(realDirection.z, realDirection.x) * Mathf.Rad2Deg;
        _angle = Mathf.LerpAngle(transform.rotation.z, _angle, Time.time);
        transform.rotation = new Quaternion(transform.eulerAngles.x, _angle, transform.eulerAngles.z, 1);
        */
        transform.LookAt(_direction, Vector3.up);
    }


    private void OnDrawGizmosSelected()
    {
        var position = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, DropPosition.position);

        Gizmos.color = new Color(1, 0.5f, 0.5f, 1);
        Gizmos.DrawLine(position, PickupPosition.position);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(position, new Vector3(0.1f, 0.3f, 0.5f));
        
    }
}
