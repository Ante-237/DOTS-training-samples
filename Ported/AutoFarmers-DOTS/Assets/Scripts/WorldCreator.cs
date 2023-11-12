using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using Random = UnityEngine.Random;

public class WorldCreator : MonoBehaviour
{
   // number of objects to spawn
   // spawning objects at random positions
   // reference to gameobject to spawn
   [SerializeField] [Range(1, 1000)] private int numberOfWorkers;
   [SerializeField] [Range(1, 1000)] private int numberOfPickups;
   [SerializeField] [Range(1, 1000)] private int numberOfDrops;
   [SerializeField] private GameObject workerMan;
   [SerializeField] private GameObject Pickups;
   [SerializeField] private GameObject Dropoffs;


   private List<GameObject> pickupsList = new List<GameObject>();
   private List<GameObject> DropOffList = new List<GameObject>();

   private int _currentPickupSize = 0;
   private int _currentDropOffSize = 0;
   private WorkerMan _cacheWorkerMan;

   private void Start()
   { 
      for (int i = 0; i < numberOfDrops; i++)
      {
         Vector2 location = Random.insideUnitCircle * 20;
         GameObject current = Instantiate(Dropoffs, new Vector3(location.x, 0.25f, location.y), Quaternion.identity);
         DropOffList.Add(current);
      }
      _currentDropOffSize = DropOffList.Count;
      
      for (int i = 0; i < numberOfPickups; i++)
      {
         Vector2 location = Random.insideUnitCircle * 30;
         GameObject current = Instantiate(Pickups, new Vector3(location.x, 0.25f, location.y), Quaternion.identity);
         pickupsList.Add(current);
      }
      _currentPickupSize = pickupsList.Count;
      
      
      for (int i = 0; i < numberOfWorkers; i++)
      {
         Vector2 location = Random.insideUnitCircle;
         GameObject current = Instantiate(workerMan, new Vector3(location.x, 0, location.y), Quaternion.identity);
         _cacheWorkerMan = current.GetComponent<WorkerMan>();
         _cacheWorkerMan.WorkerSpeed = Random.Range(1, 20f);
         _cacheWorkerMan.DropPosition = DropOffList[Random.Range(0, _currentDropOffSize)].transform;
         _cacheWorkerMan.PickupPosition = pickupsList[Random.Range(0, _currentPickupSize)].transform;
      }
   }

 
}
