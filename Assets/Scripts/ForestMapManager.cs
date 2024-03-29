﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MapStateManager is the place to keep a succession of events or "states" when building 
/// a multi-step AI demo. Note that this is a way to manage 
/// 
/// State changes could happen for one of two reasons:
///     when the user has pressed a number key 0..9, desiring a new phase
///     when something happening in the game forces a transition to the next phase
/// 
/// One use will be for AI demos that are switched up based on keyboard input. For that, 
/// the number keys 0..9 will be used to dial in whichever phase the user wants to see.
/// </summary>

public class ForestMapManager : MonoBehaviour {
    // Set prefabs
    public GameObject PlayerPrefab;     // You, the player
    public GameObject LittleBirdPrefab;     // Agent doing chasing
    public GameObject BirdKingPrefab;       // Agent getting chased
    public GameObject RedPrefab;        // Red Riding Hood, or just "team red"
    public GameObject BluePrefab;       // "team blue"
    public GameObject TreePrefab;       // New for Assignment #2

    public NPCController house;         // for future use

    // Set up to use spawn points. Can add more here, and also add them to the 
    // Unity project. This won't be a good idea later on when you want to spawn
    // a lot of agents dynamically, as with Flocking and Formation movement.

    public GameObject spawner1;
    public Text SpawnText1;
    public GameObject spawner2;
    public Text SpawnText2;
    public GameObject spawner3;
    public Text SpawnText3;

    public int TreeCount;
 
    private List<GameObject> spawnedNPCs;   // When you need to iterate over a number of agents.
    //private List<GameObject> trees;

    //private int currentPhase = 0;           // This stores where in the "phases" the game is.
    //private int previousPhase = 0;          // The "phases" we were just in

    //public int Phase => currentPhase;
    StateController stateController;
    LineRenderer line;                 
    public GameObject[] Path;
    public Text narrator;                   // 

    // Use this for initialization. Create any initial NPCs here and store them in the 
    // spawnedNPCs list. You can always add/remove NPCs later on.

    public Vector3 RandomPosition(int xlb, int xub, int zlb, int zub)
    {
        bool found = false;
        Vector3 newPos = new Vector3(0f, 1f, 0f);

        while (found == false)
        {
            float x = UnityEngine.Random.Range(xlb, xub);
            float z = UnityEngine.Random.Range(zlb, zub);
            newPos = new Vector3(x, 1f, z);
            Collider[] hitColliders = Physics.OverlapSphere(newPos, .5f);
            if (hitColliders.Length == 0)
            {
                found = true;
                break;
            }
        }
        return newPos;
    }

    void Start() {
        
        narrator.text = "This is the place to mention major things going on during the demo, the \"narration.\"";
        stateController = GameObject.FindGameObjectWithTag("GameController").GetComponent<StateController>();
        spawnedNPCs = new List<GameObject>();
        GameObject[] flock = new GameObject[5];
        //SPAWN FIRST LEADER BIRD. spawnedNPCs[0]
        spawner1.transform.position = new Vector3(5, 1, 13);
        GameObject BirdKing = SpawnItem(spawner1, BirdKingPrefab, null, SpawnText1, 3);
        BirdKing.transform.position = new Vector3(5f, 1f, 12f);
        BirdKing.tag = "BK3";
        spawnedNPCs.Add(BirdKing);
        //SPAWNING FIRST GROUP OF BIRDS. spawnedNPCs[1,2,3,4,5]
        for (int i = 0; i < 5; i++)
        {
            //RandomPosition(-23, 23, -19, 19) will spawn across the whole map
            spawner1.transform.position = RandomPosition(5, 6, 10, 11);
            //LITTLE BIRDS FOLLOW THE LEADER
            GameObject bird = SpawnItem(spawner1, LittleBirdPrefab, BirdKing.GetComponent<NPCController>(), SpawnText1, 6);
            spawnedNPCs.Add(bird);
            flock[i] = spawnedNPCs[i];
        }
        for (int j = 0; j < 6; j++)
        {
            spawnedNPCs[j].GetComponent<SteeringBehavior>().flock = flock;
        }
        ClearStage();
        EnterMapStateThree();
    }
    public void ClearStage()
    {
        foreach (GameObject NPC in spawnedNPCs)
        {
            //NPC.GetComponent<NPCController>().label.enabled = false;
            NPC.SetActive(false);
        }
    }
    private void Update()
    {
        string inputstring = Input.inputString;
        if (inputstring.Length > 0)
        {
            if (inputstring[0] == 'R' || inputstring[0] == 'r')
            {
                stateController.LoadThree();
            }
            if (inputstring[0] == 'S' || inputstring[0] == 's')
            {
                foreach (GameObject NPC in spawnedNPCs)
                {
                    //NPC.GetComponent<NPCController>().label.enabled = true;
                    NPC.SetActive(true);
                }
            }
        }
            
    }

    public void EnterMapStateThree()
    {
        narrator.text = "Birds are trying their best to follow the leader";
        CreatePath();
    }

    /// <summary>
    /// SpawnItem placess an NPC of the desired type into the game and sets up the neighboring 
    /// floating text items nearby (diegetic UI elements), which will follow the movement of the NPC.
    /// </summary>
    /// <param name="spawner"></param>
    /// <param name="spawnPrefab"></param>
    /// <param name="target"></param>
    /// <param name="spawnText"></param>
    /// <param name="phase"></param>
    /// <returns></returns>
    private GameObject SpawnItem(GameObject spawner, GameObject spawnPrefab, NPCController target, Text spawnText, int phase)
    {
        Vector3 size = spawner.transform.localScale;
        Vector3 position = spawner.transform.position + new Vector3(UnityEngine.Random.Range(-size.x / 2, size.x / 2), 0, UnityEngine.Random.Range(-size.z / 2, size.z / 2));
        GameObject temp = Instantiate(spawnPrefab, position, Quaternion.identity);
        if (target)
        {
            temp.GetComponent<SteeringBehavior>().target = target;
        }
        temp.GetComponent<NPCController>().label = spawnText;
        temp.GetComponent<NPCController>().phase = phase;
        Camera.main.GetComponent<CameraController>().player = temp;
        return temp;
    }

    /*
    /// <summary>
    /// SpawnTrees will randomly place tree prefabs all over the map. The diameters
    /// of the trees are also varied randomly.
    /// 
    /// Note that it isn't particularly smart about this (yet): notably, it doesn't
    /// check first to see if there is something already there. This should get fixed.
    /// </summary>
    /// <param name="numTrees">desired number of trees</param>
    private void SpawnTrees(int numTrees)
    {
        float MAX_X = 25;  // Size of the map; ideally, these shouldn't be hard coded
        float MAX_Z = 20;
        float less_X = MAX_X - 1;
        float less_Z = MAX_Z - 1;

        float diameter;

        for (int i = 0; i < numTrees; i++)
        {
            //Vector3 size = spawner.transform.localScale;
            Vector3 position = new Vector3(UnityEngine.Random.Range(-less_X, less_X), 0, UnityEngine.Random.Range(-less_Z, less_Z));
            GameObject temp = Instantiate(TreePrefab, position, Quaternion.identity);

            // diameter will be somewhere between .2 and .7 for both X and Z:
            diameter = UnityEngine.Random.Range(0.2F, 0.7F);
            temp.transform.localScale = new Vector3(diameter, 1.0F, diameter);

            trees.Add(temp);
          
        }
    }
    */

    /*
    private void DestroyTrees()
    {
        GameObject temp;
        for (int i = 0; i < trees.Count; i++)
        {
            temp = trees[i];
            Destroy(temp);
        }
        // Following this, write whatever methods you need that you can bolt together to 
        // create more complex movement behaviors.
    }
    */

    private void CreatePath()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = Path.Length;
        for (int i = 0; i < Path.Length; i++)
        {
            line.SetPosition(i, Path[i].transform.position);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(spawner1.transform.position, spawner1.transform.localScale);
        Gizmos.DrawCube(spawner2.transform.position, spawner2.transform.localScale);
        Gizmos.DrawCube(spawner3.transform.position, spawner3.transform.localScale);
    }
}
