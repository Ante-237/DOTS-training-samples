using Components;
using Unity.Entities;

namespace Authoring
{
    public class TestSplineAuthoring: UnityEngine.MonoBehaviour
    {
        public UnityEngine.GameObject CarPrefab;
    }

    public class TestSplineBaker : Baker<TestSplineAuthoring>
    {
        public override void Bake(TestSplineAuthoring authoring)
        {
            AddComponent(new TestSplineConfig()
                {
                    CarPrefab = GetEntity(authoring.CarPrefab)
                } );
        }
    }
}