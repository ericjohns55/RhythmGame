using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public class ScoreCheck : MonoBehaviour
{
    float noteTime;
    int noteID;

    List<KeyCode> keys = new List<KeyCode>() {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon
    };


    // Start is called before the first frame update
    void Start()
    {
        float latency = PlayerPrefs.GetFloat("Total_Latency");
    }

    // Update is called once per frame
    void Update()
    {
        
        if(keys.Exists)
    }

    public void SetNoteTime(float time)
    {
        noteTime = time;
    }

    public void SetNoteID(int id)
    {
        noteID = id;
    }
}
