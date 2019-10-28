using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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

public class FieldMapManager : MonoBehaviour {
    // Set prefabs
    public GameObject PlayerPrefab;     // You, the player
    public GameObject LittleBirdPrefab;     // Flocking members
    public GameObject WolfPrefab;       // Agent getting chased
    public GameObject RedPrefab;        // Red Riding Hood, or just "team red"
    public GameObject BluePrefab;       // "team blue"
    public GameObject TreePrefab;       // New for Assignment #2

    public NPCController house;         // for future use
    public NPCController player;

    private StateController stateController;
    // Set up to use spawn points. Can add more here, and also add them to the 
    // Unity project. This won't be a good idea later on when you want to spawn
    // a lot of agents dynamically, as with Flocking and Formation movement.

    public GameObject spawner1;
    public Text SpawnText1;
    public GameObject spawner2;
    public Text SpawnText2;
    public GameObject spawner3;
    public Text SpawnText3;
    public int CurrentNum;

    private List<GameObject> spawnedNPCs;   // When you need to iterate over a number of agents.
    //private int currentPhase = 0;           // This stores where in the "phases" the game is.
    //private int previousPhase = 0;          // The "phases" we were just in

    //public int Phase => currentPhase;
    LineRenderer line1;
    LineRenderer line2;
    public GameObject LR2;
    public GameObject[] Path1;
    public GameObject[] Path2;
    public Text narrator;                   // 
    
    // Use this for initialization. Create any initial NPCs here and store them in the 
    // spawnedNPCs list. You can always add/remove NPCs later on.
    public void ClearStage()
    {
        foreach (GameObject NPC in spawnedNPCs)
        {
            //NPC.GetComponent<NPCController>().label.enabled = false;
            NPC.SetActive(false);
        }
    }
    
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
        //narrator.text = "This is the place to mention major things going on during the demo, the \"narration.\"";
        stateController = GameObject.FindGameObjectWithTag("GameController").GetComponent<StateController>();
        CurrentNum = stateController.statenum;

        GameObject[] flock = new GameObject[3];
        //Spawn 20 agents who follow the player
        for (int i = 0; i < 3; i++)
        {
            //RandomPosition(-23, 23, -19, 19) will spawn across the whole map
            spawner1.transform.position = RandomPosition(-5, 5, -5, 5);
            spawnedNPCs.Add(SpawnItem(spawner1, LittleBirdPrefab, player, SpawnText1, 1));
            flock[i] = spawnedNPCs[i];
        }
        player.GetComponent<SteeringBehavior>().flock = flock;
        //flock[3] = GameObject.FindGameObjectWithTag("Player");
        //player.GetComponent<NPCController>().phase = 2;
        //ClearStage();
        for (int j = 0; j < 3; j++)
        {
            spawnedNPCs[j].GetComponent<SteeringBehavior>().flock = flock;
        }





        flock = new GameObject[5];
        //SPAWN FIRST LEADER BIRD. spawnedNPCs[20]
        spawner1.transform.position = new Vector3(-20, 1, 10);
        GameObject BirdKing1 = SpawnItem(spawner1, LittleBirdPrefab, null, SpawnText1, 3);
        spawnedNPCs.Add(BirdKing1);
        //SPAWNING FIRST GROUP OF BIRDS. spawnedNPCs[21,22,23,24,25]
        for (int i = 0; i < 5; i++)
        {
            //RandomPosition(-23, 23, -19, 19) will spawn across the whole map
            spawner1.transform.position = RandomPosition(-24, -20, 10, 14);
            //LITTLE BIRDS FOLLOW THE LEADER
            spawnedNPCs.Add(SpawnItem(spawner1, LittleBirdPrefab, BirdKing1.GetComponent<NPCController>(), SpawnText1, 1));
            flock[i] = spawnedNPCs[i];
        }
        BirdKing1.GetComponent<SteeringBehavior>().flock = flock;
        for (int j = 21; j < 26; j++)
        {
            spawnedNPCs[j].GetComponent<SteeringBehavior>().flock = flock;
        }




        flock = new GameObject[5];
        //SPAWN Second LEADER BIRD spawnedNPCs[26]
        spawner1.transform.position = new Vector3(-20, 1, -10);
        GameObject BirdKing2 = SpawnItem(spawner1, LittleBirdPrefab, null, SpawnText1, 3);
        spawnedNPCs.Add(BirdKing2);
        //SPAWNING Second GROUP OF BIRDS. spawnedNPCs[27,28,29,30,31]
        for (int i = 0; i < 5; i++)
        {
            //RandomPosition(-23, 23, -19, 19) will spawn across the whole map
            spawner1.transform.position = RandomPosition(-24, -20, -14, -10);
            //LITTLE BIRDS FOLLOW THE LEADER
            spawnedNPCs.Add(SpawnItem(spawner1, LittleBirdPrefab, BirdKing2.GetComponent<NPCController>(), SpawnText1, 1));
            flock[i] = spawnedNPCs[i];
        }
        BirdKing2.GetComponent<SteeringBehavior>().flock = flock;
        for (int j = 27; j < 32; j++)
        {
            spawnedNPCs[j].GetComponent<SteeringBehavior>().flock = flock;
        }

        ClearStage();





        spawnedNPCs = new List<GameObject>();
        if(CurrentNum == 1)
        {
            EnterMapStateOne();
        }
        else if (CurrentNum == 2)
        {
            EnterMapStateTwo();
        }
    }




    private void Update()
    {
        string inputstring = Input.inputString;
        //Start
        if (inputstring.Length > 0)
        {
            if (inputstring[0] == 'S' | inputstring[0] == 's')
            {
                foreach (GameObject NPC in spawnedNPCs)
                {
                    //NPC.GetComponent<NPCController>().label.enabled = true;
                    NPC.SetActive(true);
                }
            }
            if (inputstring[0] == 'R' | inputstring[0] == 'r')
            {
                SceneManager.LoadScene("Field");
            }
            if(inputstring[0] == 'C')
            {
                //Cone check
            }
            if(inputstring[0] == 'P')
            {
                //Collision prediction
            }
        }
    }


    /// <summary>
    /// This is where you put the code that places the level in a particular phase.
    /// Unhide or spawn NPCs (agents) as needed, and give them things (like movements)
    /// to do. For each case you may well have more than one thing to do.
    /// </summary>
    public void EnterMapStateOne() {
        narrator.text = "Birds are following the player and do flocking behaviour";
        for (int i = 0; i < 20; i++)
        {
            spawnedNPCs[i].SetActive(true);
        }

    }

    public void EnterMapStateTwo()
    {
        narrator.text = "Entering Phase Two";
        CreatePath1();
        CreatePath2();
        for (int i = 20; i < 32; i++)
        {
            spawnedNPCs[i].SetActive(true);
        }
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

    private void CreatePath1()
    {
        line1 = GetComponent<LineRenderer>();
        line1.positionCount = Path1.Length;
        for (int i = 0; i < Path1.Length; i++)
        {
            line1.SetPosition(i, Path1[i].transform.position);
        }
    }

    private void CreatePath2()
    {
        line2 = LR2.GetComponent<LineRenderer>();
        line2.positionCount = Path2.Length;
        for (int i = 0; i < Path2.Length; i++)
        {
            line2.SetPosition(i, Path2[i].transform.position);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(spawner1.transform.position, spawner1.transform.localScale);
        Gizmos.DrawCube(spawner2.transform.position, spawner2.transform.localScale);
        Gizmos.DrawCube(spawner3.transform.position, spawner3.transform.localScale);
    }
}
