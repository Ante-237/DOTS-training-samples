using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(GravitySystem))]
public class OrientScaleSystem : SystemBase
{
   
    //[ReadOnly]
    //private ComponentDataFromEntity<Translation> cdfe;

    protected override void OnUpdate()
    {
        var upVector = new float3(0,1f,0);
        Entities
            .WithAll<IsOriented>()
            .ForEach((ref Rotation rotation,in Velocity velocity) =>
            {
                var right = math.cross(velocity.Value, upVector);
                var beeUpVector = math.cross(right, velocity.Value);
                rotation.Value = quaternion.LookRotation(velocity.Value, beeUpVector);
            }).Schedule();

        Entities
            .WithAll<IsStretched>()
            .ForEach((ref NonUniformScale scale, in Velocity velocity) =>
            {
                    //scale.Value.z = math.length(velocity.Value) / 7;       
            }).Schedule();
    }
}
