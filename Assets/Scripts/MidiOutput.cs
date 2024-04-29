using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tools;
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

    private float timestamp = 0f;
    private float timecheck = 0f;

    private OutputDevice outputDevice;
    private Playback playback;

    // public GameObject scoreManager;
    public float delay;

    public GameObject Instructions;
    [SerializeField]
    private TMP_Text countdown;
    private float countFrom = 3;
    private bool startUp = true;

    private MapGenerator generator;
    private LinkedListNode<MapEvent> currentNode = null;
    LinkedList<MapEvent> generatedMap = null;
    private MapDifficulty difficulty = MapDifficulty.Easy;

    // Needed for progressbar
    private ProgressBar progressBar;
    public GameManager gameManager;
 
    private float executionTime = 0f;

    private float waitAmount = 0.0f;

    // Made static for the purposes of testing MIDI file merging
    private static MidiFile testMidi;

    private static MidiFile blankMidi;

    // This MIDI file will be a fusion of the blank MIDI and the MIDI to be played
    private MidiFile modifiedMidi;

    void Start()
    {
        PlayerPrefs.SetInt("ViewInstructions", 0); //This line is just for demonstration to the team
        // Checks if the viewer has ever viewed the automatic instructions before and if 
        // not, shows them and stops the game until the player hits the okay button.
        if(PlayerPrefs.GetInt("ViewInstructions") == 0)
        {
            
            Instructions.SetActive(true);
            Time.timeScale = 0f;
            PlayerPrefs.SetInt("ViewInstructions", 1);
        }

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
        blankMidi = MidiFile.Read("Assets/SystemMIDIs/blank.mid");

        IEnumerable<MidiFile> midis = GetMidis();//.getEnumerator();
        midis = new[] { blankMidi, testMidi };
        

        //modifiedMidi = MergeSequentially();
        modifiedMidi = midis.MergeSequentially();
        
        outputDevice = OutputDevice.GetByIndex(0);
        playback = testMidi.GetPlayback(outputDevice);

        // generate the map for our test level

        generator = new MapGenerator(modifiedMidi);
        
        difficulty = MapDifficulty.Easy;

        StartCoroutine(CountdownStart());
    }

    /**
    * This coroutine governs the countdown at the start of the game. It will countdown from
    * three then display a message. The startUp boolean disables and enables the playback
    * so it cannot begin until the countdown is complete.
    */
    IEnumerator CountdownStart()
    {
        countdown.gameObject.SetActive(true);
        startUp = true;
        while(countFrom > 0)
        {
            countdown.SetText(countFrom.ToString());

            yield return new WaitForSeconds(1f);

            countFrom --;
        }

        countdown.SetText("Let's Rhythm!");
        yield return new WaitForSeconds(1f);
        countdown.gameObject.SetActive(false);
        startUp = false;
        string difficultyString = PlayerPrefs.GetString(DifficultySelector.DifficultyKey, "Easy");
        if(Enum.TryParse(difficultyString, out MapDifficulty parsedDifficulty))
        {
            difficulty = parsedDifficulty;
        }

        if (PlayerPrefs.GetInt(DifficultySelector.GhostKey) == 1) {
            Debug.Log("Enabling ghost notes for Map Generation");
            generator.enableGhostNotes();
        } else {
            Debug.Log("Ghost notes will not be enabled for this MIDI");
        }
        if (Time.time > timestamp + 0.50f && startUp == false) // startUp is checked for here 
        {
            
            GameSetup();
        }
    }

    public static IEnumerable<MidiFile> GetMidis()
    {
        yield return blankMidi;
        yield return testMidi;
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

    }

    private void GameSetup()
    {      
            timestamp = Time.time;
            timecheck = timestamp;
            //These assignments clear the note display TMPs

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
                    gameManager.SetNoteCount(generator.GetNoteCount());
                    gameManager.SetSongEndDelay(generator.GetSongEndDelay());
                }
                
                currentNode = generatedMap.First;   
                waitAmount = 0.0f;                 

                // testFlag = true;
                StartCoroutine(BeginMidiPlayback());

                executionTime = Time.time;
                StartCoroutine(SpawnNotes());
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
           
            spriteCreator.generateNote(noteID, currentEvent.GetGhostNote());
            // Gives ScoreCheck the ID of the current note being played
            // scoreManager.GetComponent<ScoreCheck>().SetNoteID(noteID);
            //Debug.Log(noteID + " time: " + Time.time);
        }

        // THIS ONE GOVERNS TIME BETWEEN BLOCKS. NEEDS TO BE BASED ON TIME BETWEEN NOTES
        //yield return new WaitForSeconds(.75f);

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

    public void StartCountdown()
    {
        Time.timeScale = 1f;
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
            if (playback.IsRunning) {
                playback.Stop();
            }
            
            playback.Dispose();
        }

        if (outputDevice != null) {
            outputDevice.Dispose();
        }
    }
}
