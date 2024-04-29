using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine.SceneManagement;

public class LatencyPress : MonoBehaviour
{
    public GameObject first;
    public GameObject second;
    public GameObject third;
    public GameObject fourth;
    public GameObject fifth;
    public GameObject sixth;
    public GameObject seventh;

   // public UnityEngine.UI.Button continueButton;

   // public UnityEngine.UI.Button exitButton;

    public GameObject step1;
    public GameObject step2;
    public GameObject info3;
    public TMP_Text text3;

    public GameObject hit;

    private List<GameObject> beatQueue;

    public static float timeAdjust = 1000;
    private static int listTracker = 0;
    private static int testPasses = 0;
    private float[] pressTime = new float[5];
    private float[] noteTime = new float[5];

    private float[] latencyTime = new float[5];

    private OutputDevice outputDevice;
    private Playback playback;

    void Start()
    {
        beatQueue = new List<GameObject>
        {
            first,
            second,
            third,
            fourth,
            fifth,
            sixth,
            seventh
        };

        timeAdjust = PlayerPrefs.GetFloat("AV_Latency");

        MidiFile testMidi = MidiFile.Read("Assets/SystemMIDIs/latency3.mid");
        outputDevice = OutputDevice.GetByIndex(0);
        playback = testMidi.GetPlayback(outputDevice);
        
        playback.Loop = true;
        Debug.Log("Starting playback");
        playback.Start();


        //Debug.Log("Coroutine start");
        StartCoroutine(BlinkCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {   if(testPasses < 5)
            {
                pressTime[testPasses] = Time.time;
                hit.SetActive(true);
            }
        }

        //hit.SetActive(false);
    }

    IEnumerator BlinkCoroutine()
    {   Debug.Log("Coroutine start");
        beatQueue[listTracker].SetActive(false);
        if(testPasses < 5) 
        {  
            if(listTracker == 0)
            {
                beatQueue[6].SetActive(true);
            }
            else
            {
                beatQueue[listTracker - 1].SetActive(true);
                if(listTracker == 6)
                {
                    noteTime[testPasses] = Time.time;
                }
            }

            yield return new WaitForSeconds(timeAdjust / 1000);

            if(listTracker == 6)
            {
                listTracker = 0;
                hit.SetActive(false);
            }
            else
                listTracker += 1;

            if(listTracker == 6)
            {
                testPasses++;
            }

            StartCoroutine(BlinkCoroutine());
        }
        else
        {
            float lsum = 0;

            for(int i = 0; i < 5; i++)
            {
                lsum += pressTime[i] - noteTime[i];
            }

            float inputLatency = lsum / 5;
            PlayerPrefs.SetFloat("Input_Latency", inputLatency);
            PlayerPrefs.SetFloat("Total_Latency", timeAdjust + inputLatency);

            playback.Stop();
            playback.Dispose();
            outputDevice.Dispose();
            step2.SetActive(false);
            info3.SetActive(true);
            text3.SetText("AV Latency: " + timeAdjust + "\nInput Latency: " + inputLatency);

        }    
    }

    /**
    * The following function clears and releases the midi player upon closing the application.
    */
    void OnApplicationQuit() {
        if (playback != null) {
            
            playback.Dispose();
        }

        if (outputDevice != null) {
            outputDevice.Dispose();
        }
    }

    public void finishTest()
    {
        // WARNING: This needs to be changed to access the main menu by name rather
        //          than by scene index!
        playback.Stop();
        playback.Dispose();
        outputDevice.Dispose();
        SceneManager.LoadScene(0);
    }

    public void exitScene()
    {
        SceneManager.LoadScene(0);
    }
}
