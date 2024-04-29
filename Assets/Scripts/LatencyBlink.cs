using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using UnityEngine.SceneManagement;

public class LatencyBlink : MonoBehaviour
{
    public GameObject first;
    public GameObject second;
    public GameObject third;
    public GameObject fourth;
    public GameObject fifth;
    public GameObject sixth;
    public GameObject seventh;

    public UnityEngine.UI.Button continueButton;

    public UnityEngine.UI.Button exitButton;

    public GameObject step1;
    public GameObject step2;

    private List<GameObject> beatQueue;

    public static float timeAdjust = 1000;

    private static float timerTracker = 1000;
    private static int listTracker = 0;

    [SerializeField]
    private UnityEngine.UI.Slider slider;

    [SerializeField]
    private TMP_Text sliderText;
    private OutputDevice outputDevice;
    private Playback playback;

    void Start()
    {
        beatQueue = new List<GameObject>{};
        beatQueue.Add(first);
        beatQueue.Add(second);
        beatQueue.Add(third);
        beatQueue.Add(fourth);
        beatQueue.Add(fifth);
        beatQueue.Add(sixth);
        beatQueue.Add(seventh);

        MidiFile testMidi = MidiFile.Read("Assets/SystemMIDIs/latency3.mid");
        outputDevice = OutputDevice.GetByIndex(0);
        playback = testMidi.GetPlayback(outputDevice);

        StartCoroutine(InitialWait());
        
        playback.Loop = true;
        Debug.Log("Starting playback");
        playback.Start();

        StartCoroutine(BlinkCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        slider.onValueChanged.AddListener((v) => { timeAdjust = v; sliderText.text = v.ToString("0.00");});

        if(timerTracker != timeAdjust)
        {
            timerTracker = timeAdjust;
        }
    }

    IEnumerator InitialWait()
    {
        yield return new WaitForSeconds(5);
    }

    IEnumerator BlinkCoroutine()
    {   //Debug.Log("Coroutine start");
        beatQueue[listTracker].SetActive(false);
        
        if(listTracker == 0)
        {
            beatQueue[6].SetActive(true);
        }
        else
        {
            beatQueue[listTracker - 1].SetActive(true);
        }

        yield return new WaitForSeconds(timeAdjust / 1000);

        if(listTracker == 6)
            listTracker = 0;
        else
            listTracker += 1;

        //Debug.Log("Coroutine finished-- List tracker: " + listTracker);
        StartCoroutine(BlinkCoroutine());
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

    public void nextStep()
    {
        Debug.Log("Continue Button Works");
        PlayerPrefs.SetFloat("AV_Latency", timeAdjust);
        playback.Stop();
        playback.Dispose();
        outputDevice.Dispose();
        step1.SetActive(false);
        step2.SetActive(true);
    }

    public void exitScene()
    {
        // WARNING: This needs to be changed to access the main menu by name rather
        //          than by scene index!
        playback.Stop();
        playback.Dispose();
        outputDevice.Dispose();
        SceneManager.LoadScene(0);
    }
}
