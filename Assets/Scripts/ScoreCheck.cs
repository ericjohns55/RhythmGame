using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;
using UnityEngine.UIElements;
using System.Windows.Input;
using System;

public class ScoreCheck : MonoBehaviour
{
    float noteTime;
    float pressTime;
    int noteID;
    int noteRetrieve;
    float latency;

    Dictionary<KeyCode, int> keys = new Dictionary<KeyCode, int>(){
        {KeyCode.A, 1}, {KeyCode.S, 2}, {KeyCode.D, 3}, {KeyCode.F, 4}, {KeyCode.J, 5},
        {KeyCode.K, 6}, {KeyCode.L, 7}, {KeyCode.Semicolon, 8}
    };


    // Start is called before the first frame update
    void Start()
    {
        latency = PlayerPrefs.GetFloat("Total_Latency");
    }

    // Update is called once per frame
    void Update()
    {
        // Left off here. Need to match the note being played to the proper key then set 
        // up calculation logic of scoring range. Note offset in game must be tweaked
        foreach(KeyValuePair<KeyCode, int> key in keys)
        {
            if(Input.anyKey)
            {
                 foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
                {  
                    if(keys.ContainsKey(keyCode))
                    {
                        keys.TryGetValue(keyCode, out noteRetrieve);

                        if(noteRetrieve == noteID)
                        {
                            pressTime = Time.time;
                            ScoreRegister(pressTime);
                        }
                    }
                }   
            }
        }
    }

    public void ScoreRegister(float presstime)
    {
        float range;

        if(latency > 0)
        {
            range = noteTime - (presstime - latency);
        }
        else
        {
            range = noteTime - (presstime + latency);
        }

        float absRange = Math.Abs(range);

        if(absRange < 0.2) //perfect
        {

        }
        else if(absRange < 0.5) //good
        {
            
        }
        else if(absRange < 0.7) //bad
        {

        }
        else //miss
        {

        }
    }

    public void SetNoteTime(float time)
    {
        noteTime = time;
    }


    //this may not be necessary
    public void SetNoteID(int id)
    {
        noteID = id;
    }
}
