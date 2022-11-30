using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

readonly partial struct ConfigAspect : IAspect
{
    private readonly RefRO<Config> Config;

    public Entity WallPrefab
    {
        get => Config.ValueRO.WallPrefab;
    }

    public int PlaySize
    {
        get => Config.ValueRO.PlaySize;
    }

    public int WallCount
    {
        get => Config.ValueRO.AmountOfWalls;
    }

    public Entity FoodPrefab
    {
        get => Config.ValueRO.FoodPrefab;
    }

    public Entity ColonyPrefab
    {
        get => Config.ValueRO.ColonyPrefab;
    }
}