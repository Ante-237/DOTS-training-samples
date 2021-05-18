using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityCamera = UnityEngine.Camera;
using UnityGameObject = UnityEngine.GameObject;
using UnityInput = UnityEngine.Input;
using UnityKeyCode = UnityEngine.KeyCode;
using UnityMeshRenderer = UnityEngine.MeshRenderer;
using UnityMonoBehaviour = UnityEngine.MonoBehaviour;
using UnityRangeAttribute = UnityEngine.RangeAttribute;

public class InputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var gameConfig = GetSingleton<GameConfig>();
        
        var mousePos = UnityCamera.main.ScreenPointToRay(Input.mousePosition);
        var mouseDown = Input.GetMouseButtonDown(0);

        Entities.ForEach((ref PlayerInput playerInput, in PlayerIndex playerIndex) => {

            if (playerIndex.Index == 0)
            {
                playerInput.TileIndex = RaycastCellDirection(mousePos, gameConfig);
                playerInput.isMouseDown = mouseDown;
                
                if(mouseDown) Debug.Log(playerInput.TileIndex);
            }
            else
            {
                
            }
            
        }).Schedule();
    }
    
    public static int RaycastCellDirection(Ray ray, GameConfig gameConfig)
    {
        //cellDirection = Cardinals.North;

        float enter;

        var plane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));

        if (!plane.Raycast(ray, out enter))
            return -1;

        var worldPos = ray.GetPoint(enter);
        var cell = CellAtWorldPosition(worldPos, gameConfig);

        // var pt = cell.transform.InverseTransformPoint(worldPos);
        //
        // if (Mathf.Abs(pt.z) > Mathf.Abs(pt.x))
        //     cellDirection = pt.z > 0 ? Cardinals.North : Cardinals.South;
        // else
        //     cellDirection = pt.x > 0 ? Cardinals.East : Cardinals.West;

        return cell;

    }
    
    public static int CellAtWorldPosition(Vector3 worldPosition, GameConfig gameConfig)
    {
        var localPt3D = new float3(worldPosition.x, worldPosition.y, worldPosition.z);
        var localPt = new Vector2(localPt3D.x, localPt3D.z);

        localPt += new Vector2(0.5f, 0.5f); // offset by half cellsize
        var cellCoord = new Vector2Int(Mathf.FloorToInt(localPt.x / 1), Mathf.FloorToInt(localPt.y / 1));
        return BoardSpawner.CoordintateToIndex(gameConfig, cellCoord.x, cellCoord.y);
    }
}
