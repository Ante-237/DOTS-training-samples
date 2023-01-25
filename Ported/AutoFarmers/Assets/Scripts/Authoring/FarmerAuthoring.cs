using Unity.Entities;
using Unity.Mathematics;

class FarmerAuthoring : UnityEngine.MonoBehaviour
{
    class FarmerBaker : Baker<FarmerAuthoring>
    {
        public override void Bake(FarmerAuthoring authoring)
        {
            AddComponent(new Farmer()
            {
                moveSpeed = 10.0f,
                moveTarget = new float3(0, 0, 0),
                farmerState = FarmerStates.FARMER_STATE_HARVEST,
                backpackOffset = new float3(0, 2, 0)
            });
        }
    }
}

struct Farmer : IComponentData
{
    public float3 moveTarget;
    public float moveSpeed;
    public byte farmerState;
    public float3 backpackOffset;
    public bool holdingEntity;
    public Entity heldEntity;
    public bool hasTarget;
    public Entity currentlyTargeted;
}

public static class FarmerStates
{
    public const byte FARMER_STATE_IDLE = 0;
    public const byte FARMER_STATE_ROCKDESTROY = 1;
    public const byte FARMER_STATE_CREATEPLOT = 2;
    public const byte FARMER_STATE_HARVEST = 3;
    public const byte FARMER_STATE_PLACEINSILO = 4;
    public const byte FARMER_STATE_PLANTCROP = 5;
}