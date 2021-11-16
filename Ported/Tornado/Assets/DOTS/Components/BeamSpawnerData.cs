using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Dots
{
    [Serializable]
    public struct BeamSpawnerData : IComponentData
    {
        public int buildingCount;
    }
}
