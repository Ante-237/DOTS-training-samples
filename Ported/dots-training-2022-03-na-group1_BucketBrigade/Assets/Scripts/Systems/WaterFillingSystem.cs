﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class WaterFillingSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var time = Time.ElapsedTime;
/*
        Entities
            .ForEach((ref Translation translation, in CarMovement movement) =>
            {
                translation.Value.x = (float) ((time + movement.Offset) % 100) - 50f;
            }).ScheduleParallel();
            */
    }
}