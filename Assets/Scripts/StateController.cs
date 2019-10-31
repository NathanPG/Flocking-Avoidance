using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class StateController : MonoBehaviour
{
    private int currentPhase = 0;           // This stores where in the "phases" the game is.
    private int previousPhase = 0;          // The "phases" we were just in
    public int statenum = 0;
    public int CorP = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int num;

        string inputstring = Input.inputString;
        if (inputstring.Length > 0)
        {
            Debug.Log(inputstring);
            // Look for a number key click
            if (Int32.TryParse(inputstring, out num))
            {
                if (num != currentPhase)
                {
                    previousPhase = currentPhase;
                    currentPhase = num;
                }
            }
        }
        // Check if a game event had caused a change of phase.
        if (currentPhase == previousPhase)
            return;

        switch (currentPhase)
        {
            case 0:
                break;
            //Clicking 1 presents Part 1
            case 1:
                LoadOne();
                currentPhase = 0;
                break;
            //Clicking 2 presents Part 2
            case 2:
                LoadTwo();
                currentPhase = 0;
                break;
            //Clicking 3 presents Part 3
            case 3:
                LoadThree();
                currentPhase = 0;
                break;
            
        }
    }
    public void LoadOne()
    {
        SceneManager.LoadScene("Field");
        statenum = 1;
    }
    public void LoadTwo()
    {
        SceneManager.LoadScene("Field");
        statenum = 2;
    }
    public void LoadThree()
    {
        SceneManager.LoadScene("Forest");
        statenum = 3;
    }
}
