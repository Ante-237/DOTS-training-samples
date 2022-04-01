using Unity.Entities;
using Unity.Mathematics;

public static class BucketBrigadeUtility
{
    public const float FreeSpeed = 12f;
    public const float EmptyBucketSpeed = 12f;
    public const float FullBucketSpeed = 4f;
    public const float FillDelay = 2f;
    public const float EmptyWaterSize = 0.15f;
    public const float FullWaterSize = 0.4f;
    public const int FramesPerFireCheck = 120;

    private static int _frame;

    public static void IncrementFrame()
    {
        _frame++;
    }

    public static int GetCurrentFrame()
    {
        return _frame;
    }

    public static bool IsVeryClose(float2 a, float2 b)
    {
        return math.distancesq(a, b) < 0.01f;
    }

    public static bool IsBucketCarried(BucketState bucketState)
    {
        switch (bucketState)
        {
            case BucketState.FullCarried:
            case BucketState.EmptyCarrried:
                return true;
        }

        return false;
    }

    public static (Entity entity, float2 position) FindClosestWater(float2 position, DynamicBuffer<WaterPoolInfo> waterInfo)
    {
        var closest = Entity.Null;
        var closestPosition = float2.zero;
        var distanceSq = float.PositiveInfinity;

        for (var i = 0; i < waterInfo.Length; i++)
        {
            var element = waterInfo[i];

            var candidateDistanceSq = math.distancesq(position, element.Position);

            if (candidateDistanceSq < distanceSq)
            {
                distanceSq = candidateDistanceSq;
                closest = element.WaterPool;
                closestPosition = element.Position;
            }
        }

        return (closest, closestPosition);
    }

    public static (Entity entity, float2 position) FindClosestBucket(float2 position, DynamicBuffer<FreeBucketInfo> bucketInfo, bool mustBeEmpty)
    {
        var closest = Entity.Null;
        var closestPosition = float2.zero;
        var distanceSq = float.PositiveInfinity;

        for (var i = 0; i < bucketInfo.Length; i++)
        {
            var element = bucketInfo[i];

            if (!mustBeEmpty || element.BucketState.Value == BucketState.EmptyOnGround)
            {
                var candidateDistanceSq = math.distancesq(position, element.BucketPosition.Value);

                if (candidateDistanceSq < distanceSq)
                {
                    distanceSq = candidateDistanceSq;
                    closest = element.BucketEntity;
                    closestPosition = element.BucketPosition.Value;
                }
            }
        }

        return (closest, closestPosition);
    }

    public static float2 FindClosestFireSpot
        (in HeatMapData heatmapData, in DynamicBuffer<HeatMapTemperature> heatmap, float2 fromPosition)
    {
        // Naive approach:
        // Computing the distance with each cell of the heatmap whose temperature is higher than 0
        // and keep the closest one.
        
        // use ints and city block distance

        var width = heatmapData.mapSideLength;

        var gridPosition = GridUtility.PlotTileCoordFromWorldPosition(fromPosition,width);

        var shortestDistanceSq = int.MaxValue;
        var closestSpot = -1;

        var index = -1;
        for (var y = 0; y < width; y++)
        {
            for (var x = 0; x < width; x++)
            {
                index++;
                if (heatmap[index] < FirePropagationSystem.HEAT_THRESHOLD )
                    continue;
                
                var yDist = math.abs(gridPosition.y - y);
                var yDistSq = yDist * yDist;
                
                var xDist = math.abs(gridPosition.x - x);
                var distanceSq = yDistSq + xDist*xDist;

                if (distanceSq < shortestDistanceSq)
                {
                    shortestDistanceSq = distanceSq;
                    closestSpot = index;
                }

                //index++;
            }

        }

        return GridUtility.PlotTileWorldPosition2DFromIndex(closestSpot, width);
    }

    public static float2 CalculateLeftArc(float2 a, float2 b, float t)
    {
        var ab = b - a;

        return a + (ab * t) + (new float2(-ab.y, ab.x) * ((1f - t) * t * 0.3f));
    }

    public static DynamicBuffer<HeatMapTemperature> GetHeatmapBuffer(ComponentSystemBase systemBase)
    {
        var heatmap = systemBase.GetSingletonEntity<HeatMapTemperature>();
        return systemBase.EntityManager.GetBuffer<HeatMapTemperature>(heatmap);
    }
    
    public static DynamicBuffer<HeatMapSplash> GetSplashmapBuffer(ComponentSystemBase systemBase) 
    {
        var splashmap = systemBase.GetSingletonEntity<HeatMapSplash>();
        return systemBase.EntityManager.GetBuffer<HeatMapSplash>(splashmap);
    }

    public static HeatMapData GetHeatmapData(ComponentSystemBase systemBase)
    {
        return systemBase.GetSingleton<HeatMapData>();
    }
}