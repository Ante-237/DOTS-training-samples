using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ConfigAuthoring : MonoBehaviour
{
    public GameObject carPrefab;
    public float2 SpeedRange;

    [Header("Highway Bounds Properties")]
    public int DesiredCarNumber = 100;
    public float HighwayMaxSize = 500;
    public int MaxCarSpawnPerFrame = 10;
    public bool AllowedToKillCar = false;
    public int NumLanes = 4;

    [Header("Car Properties")]
    public float Acceleration = 15;
    public float BrakeDeceleration = 20;
    [Tooltip("\"Speed\" in lanes on how quickly the car changes lanes.")]
    public float SwitchLanesSpeed = 3;
    [Tooltip("Give up on overtaking a car if it takes this long or more.")]
    public float OvertakeMaxDuration = 5;

    [Header("Car Bounds Properties")]
    public float DefaultSpeedMin = 15;
    public float DefaultSpeedMax = 25;
    [Tooltip("Min bound for a car's overtake percentage.  This is the percent of the car's default speed it will be at when attempting to overtake a car.")]
    public float OvertakePercentMin = 1.2f;
    [Tooltip("Max bound for a car's overtake percentage.  This is the percent of the car's default speed it will be at when attempting to overtake a car.")]
    public float OvertakePercentMax = 2f;
    [Tooltip("Min distance to slow car in front needed to start merge to the left.")]
    public float LeftMergeDistanceMin = 1;
    [Tooltip("Max distance to slow car in front needed to start merge to the left.")]
    public float LeftMergeDistanceMax = 3;
    [Tooltip("Min distance required between this car and cars in adjacent lanes to start a merge.")]
    public float MergeSpaceMin = 1;
    [Tooltip("Max distance required between this car and cars in adjacent lanes to start a merge.")]
    public float MergeSpaceMax = 3;
    [Tooltip("Min bound for eagerness; If eagerness > (car in front's speed) / (this car's speed), then this car will attempt to overtake the car in front.")]
    public float OvertakeEagernessMin = .5f;
    [Tooltip("Max bound for eagerness; If eagerness > (car in front's speed) / (this car's speed), then this car will attempt to overtake the car in front.")]
    public float OvertakeEagernessMax = 1.5f;

    class Baker : Baker<ConfigAuthoring>
    {
        public override void Bake(ConfigAuthoring authoring)
        {
            // Each authoring field corresponds to a component field of the same name.
            AddComponent(new Config
            {
                DesiredCarNumber = authoring.DesiredCarNumber,
                MaxCarSpawnPerFrame = authoring.MaxCarSpawnPerFrame,
                AllowedToKillCar = authoring.AllowedToKillCar,
                CarPrefab = GetEntity(authoring.carPrefab),
                MaxNumCars = authoring.DesiredCarNumber,
                NumLanes = authoring.NumLanes,
                HighwayMaxSize = authoring.HighwayMaxSize,
                Acceleration = authoring.Acceleration,
                BrakeDeceleration = authoring.BrakeDeceleration,
                SwitchLanesSpeed = authoring.SwitchLanesSpeed,
                OvertakeMaxDuration = authoring.OvertakeMaxDuration,
                DefaultSpeedMin = authoring.DefaultSpeedMin,
                DefaultSpeedMax = authoring.DefaultSpeedMax,
                OvertakePercentMin = authoring.OvertakePercentMin,
                OvertakePercentMax = authoring.OvertakePercentMax,
                LeftMergeDistanceMin = authoring.LeftMergeDistanceMin,
                LeftMergeDistanceMax = authoring.LeftMergeDistanceMax,
                MergeSpaceMin = authoring.MergeSpaceMin,
                MergeSpaceMax = authoring.MergeSpaceMax,
                OvertakeEagernessMin = authoring.OvertakeEagernessMin,
                OvertakeEagernessMax = authoring.OvertakeEagernessMax
            });
        }
    }
}

public struct Config : IComponentData
{
    public int DesiredCarNumber;
    public int MaxCarSpawnPerFrame;
    public bool AllowedToKillCar;
    public Entity CarPrefab;
    
    [Header("Highway Bounds Properties")]
    public int MaxNumCars;
    public float HighwayMaxSize;
    public int NumLanes;

    //Car Properties
    public float Acceleration;
    public float BrakeDeceleration;
    //"Speed" in lanes on how quickly the car changes lanes.
    public float SwitchLanesSpeed;
    //Give up on overtaking a car if it takes this long or more.
    public float OvertakeMaxDuration;

    //Car Bounds Properties
    public float DefaultSpeedMin;
    public float DefaultSpeedMax;
    //Min bound for a car's overtake percentage.  This is the percent of the car's default speed it will be at when attempting to overtake a car
    public float OvertakePercentMin;
    //Max bound for a car's overtake percentage.  This is the percent of the car's default speed it will be at when attempting to overtake a car.
    public float OvertakePercentMax;
    //Min distance to slow car in front needed to start merge to the left.
    public float LeftMergeDistanceMin;
    //Max distance to slow car in front needed to start merge to the left.
    public float LeftMergeDistanceMax;
    //Min distance required between this car and cars in adjacent lanes to start a merge.
    public float MergeSpaceMin;
    //Max distance required between this car and cars in adjacent lanes to start a merge.
    public float MergeSpaceMax;
    //Min bound for eagerness; If eagerness > (car in front's speed) / (this car's speed), then this car will attempt to overtake the car in front.
    public float OvertakeEagernessMin;
    //Max bound for eagerness; If eagerness > (car in front's speed) / (this car's speed), then this car will attempt to overtake the car in front.
    public float OvertakeEagernessMax;

}