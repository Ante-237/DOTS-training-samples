using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camFollow : MonoBehaviour
{
    public Vector2 viewAngles;
    public float viewDist;
    public float mouseSensitivity;

    void Start () {
        transform.rotation = Quaternion.Euler(viewAngles.y,viewAngles.x,0f);
    }

}
