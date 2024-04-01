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
        {KeyCode.A, 0}, {KeyCode.S, 1}, {KeyCode.D, 2}, {KeyCode.F, 3}, {KeyCode.J, 4},
        {KeyCode.K, 5}, {KeyCode.L, 6}, {KeyCode.Semicolon, 7}
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
            if(Input.anyKeyDown)
            {
                 //foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode))) //THIS NEEDS TO BE REPLACED
                //{  
                   // if(keys.ContainsKey(Input.GetKey(key)))
                    if(Input.GetKey(key.Key))
                    {
                        //Debug.Log("Mouse click should not get here");
                        keys.TryGetValue(key.Key, out noteRetrieve);

                        if(noteRetrieve == noteID)
                        {
                            Debug.Log("Hit : " + noteID + " at: " + Time.time);
                            pressTime = Time.time;
                            ScoreRegister(pressTime);
                        }
                    }
                //}   
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
            Debug.Log("Hit");
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
