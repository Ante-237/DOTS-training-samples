
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(BucketSpawningSystem))]
public partial struct BotSpawningSystem : ISystem
{
    private int numRow;
    private int numCol;
    private int totalBots;
    private Entity botPrefab;
    private Config config; 
    
    
    public void OnUpdate(ref SystemState state)
    {
        // Get the config 
        config = SystemAPI.GetSingleton<Config>();
        
        //Get the total amount of bots
        totalBots = 10;
        Debug.Log(totalBots);
        //Get the bot prefab
        botPrefab = config.Bot;
        
        //Get the rows and colums 
        numRow = config.rows;
        numCol = config.columns;
        
        //Get the random component 
        Random randomComponent = SystemAPI.GetSingleton<Random>();
        randomComponent.Value.InitState(4);

        //Get the ECB
        var ECBSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ECB = ECBSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        //Loop through and spawn at a random position
        for (int i = 0; i < totalBots; i++)
        {
            //Instatiate the bucket
            var instance = ECB.Instantiate(botPrefab);
            //Set its transform
            var bucketTransform = LocalTransform.FromPosition(
                randomComponent.Value.NextFloat(1, numRow-1) ,
                0.25f,
                randomComponent.Value.NextFloat(1, numCol-1));
            
            bucketTransform.Scale = 1f; //This is the scale of the bot pls change this
            
            ECB.SetComponent(instance,bucketTransform);
            
        }
        
        
        
        //Disable state after spawning for now 
        state.Enabled = false;
    }
}