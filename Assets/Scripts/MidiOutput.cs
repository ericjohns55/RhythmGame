using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using System.Linq;
using MapGeneration;
using System;
using System.IO;

/**
* This class currently governs midi map creation, map tracking, output device, and note creation.
* A series of Lists are implemented to keep track of notes that have been played, are currently 
* playing, and will be played as well as displaying them upon being played as a TMP.
*/
public class MidiOutput : MonoBehaviour
{
    private SpriteCreator spriteCreator;

    private string notesPlaying = "";
    private string lastPlayed = "";

    private float timestamp = 0f;
    private float timecheck = 0f;

    private OutputDevice outputDevice;
    private Playback playback;

    public TMP_Text noteLogger;
    // public GameObject scoreManager;
    public float delay;

    private MapGenerator generator;
    private LinkedListNode<MapEvent> currentNode = null;
    LinkedList<MapEvent> generatedMap = null;
    private MapDifficulty difficulty;

    // Needed for progressbar
    private ProgressBar progressBar;
    public GameManager gameManager;
 
    private float executionTime = 0f;

    private float waitAmount = 0.0f;

    private MidiFile testMidi;
    void Start()
    {
        // grab instance of SpriteCreator for note creation

        spriteCreator = Camera.main.GetComponent<SpriteCreator>();

        progressBar = (ProgressBar) gameManager.GetComponent("ProgressBar");

        string SelectedMidiFilePath = PlayerPrefs.GetString("SelectedMidiFilePath", "");
        Debug.Log(SelectedMidiFilePath);

        // parse the file name from the selected midi file
        string midiFileName = Path.GetFileNameWithoutExtension(SelectedMidiFilePath);
        Debug.Log(midiFileName);

        // load the test midi file and setup output devices and playback

        testMidi = MidiFile.Read("Assets/MIDIs/" +  midiFileName + ".mid");
        
        outputDevice = OutputDevice.GetByIndex(0);
        playback = testMidi.GetPlayback(outputDevice);

        // generate the map for our test level
        generator = new MapGenerator(testMidi);
        difficulty = MapDifficulty.Easy;
    }

    /**
    * The following function clears and releases the midi player upon closing the application.
    */
    void OnApplicationQuit() {
        ReleaseOutputDevice();
    }

    // Update is called once per frame
    void Update() //FixedUpdate()
    {
        //Sets the TMP element on screen to display the names of the notes being played.
        if (!notesPlaying.Equals(lastPlayed)) {
            noteLogger.text = notesPlaying;
            lastPlayed = notesPlaying;
        }

        /*
        * The following logical chain starts, stops, and resets midi playback using
        * the spacebar
        */
        if (Time.time > timestamp + 0.50f) {
            if (Input.GetKey(KeyCode.P)) {
                timestamp = Time.time;
                timecheck = timestamp;
                //These assignments clear the note display TMPs
                notesPlaying = "";
                lastPlayed = "";
                noteLogger.text = "";

                progressBar.ResetBar();

                if (playback.IsRunning) {
                    playback.Stop();

                    StopAllCoroutines();

                    // TODO: make it so we pause and unpause cleanly
                    // reset previous playback
                    currentNode = null;
                    progressBar.ResetBar();
                } else {
                    // the linked list was generated based off of a SortedDictionary, so the first note is guaranteed the first node
                    if (generatedMap == null) {
                        generatedMap = generator.GenerateMap(difficulty);
                        progressBar.SetMaxValue(generatedMap.Count);
                    }
                    
                    currentNode = generatedMap.First;   
                    waitAmount = 0.0f;                 

                    // testFlag = true;
                    StartCoroutine(BeginMidiPlayback());

                    executionTime = Time.time;
                    StartCoroutine(SpawnNotes());
                }
            }
        }
    }

    private IEnumerator BeginMidiPlayback() {
        yield return new WaitForSeconds(0.2f); // in theory this is delay
        playback.MoveToStart();
        playback.Start();
    }

    private IEnumerator SpawnNotes() {
        if (currentNode == null) yield break; // signifies either a break or the end of the song

        // frame times arent perfect, this accounts for that  that
        float offset = Time.time - executionTime - waitAmount;
                
        MapEvent currentEvent = currentNode.Value;
        foreach (int noteID in currentEvent.GetTilesToGenerate()) { // generates notes from the current map event
           
            spriteCreator.generateNote(noteID);
            // Gives ScoreCheck the ID of the current note being played
            // scoreManager.GetComponent<ScoreCheck>().SetNoteID(noteID);
            //Debug.Log(noteID + " time: " + Time.time);
        }

        // THIS ONE GOVERNS TIME BETWEEN BLOCKS. NEEDS TO BE BASED ON TIME BETWEEN NOTES
        //yield return new WaitForSeconds(.75f);
        notesPlaying = currentEvent.GetNoteList(); // debug text for current notes (will remove later)

        long currentTimestamp = currentEvent.GetTimestamp(); // grabs current timestamp for next calculation
        //Debug.Log("Note: being played at: " + Time.time);
        currentNode = currentNode.Next;

        // Gives ScoreCheck the timestamp during which the current note is being played
        // scoreManager.GetComponent<ScoreCheck>().SetNoteTime((float) currentTimestamp);
        

        if (currentNode != null) { // make sure there is a next null, otherwise we do not need to wait anymore (end of song)
            waitAmount = generator.CalculateNextTimeStamp(currentTimestamp, currentNode.Value.GetTimestamp());
            // Debug.LogFormat("Waiting {0}s for next event (TIMESTAMP {1}).", waitAmount, currentTimestamp);
            
            // Debug.Log("Delay should be: " + (Time.time - timecheck));
            
            yield return new WaitForSeconds(waitAmount - offset); 
            StartCoroutine(SpawnNotes()); // recursively call subroutine for next note
            executionTime = Time.time;
        }

        progressBar.Increment();
    }

    public void StopPlayback()
    {
        if (playback != null) {
            playback.Stop();
        }
    }

    public void StartPlayback()
    {
        if (playback != null) {
            playback.Start();
        }
    }

    public bool GetPlaybackState() {
        if (playback != null) {
            return playback.IsRunning;
        }
        return false;
    }

    public void ReleaseOutputDevice() {
        if (playback != null) {
            playback.Dispose();
        }

        if (outputDevice != null) {
            outputDevice.Dispose();
        }
    }
}
