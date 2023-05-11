﻿using Unity.Entities;
using Unity.Mathematics;


public struct Train : IComponentData
{
    public int LineID;
    public int TrainId;
    public bool OnPlatformA;
    public Entity StationEntity;
    public Entity TrackEntity;
    public int TrackPointIndex;
    public bool Forward;
    public float3 Offset;
    public float Duration;
    public float Speed;
}
