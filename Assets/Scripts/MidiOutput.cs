using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using MapGeneration;

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

    private OutputDevice outputDevice;
    private Playback playback;

    public TMP_Text noteLogger;

    private MapGenerator generator;
    private LinkedListNode<MapEvent> currentNode = null;
    private MapDifficulty difficulty;
 
    void Start()
    {
        // grab instance of SpriteCreator for note creation
        spriteCreator = Camera.main.GetComponent<SpriteCreator>();

        // load the test midi file and setup output devices and playback
        MidiFile testMidi = MidiFile.Read("Assets/MIDIs/NoteChartingSlow.mid");
        outputDevice = OutputDevice.GetByIndex(0);
        playback = testMidi.GetPlayback(outputDevice);

        // generate the map for our test level
        generator = new MapGenerator(testMidi);
        difficulty = MapDifficulty.Medium;
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

    // Update is called once per frame
    void FixedUpdate()
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
            if (Input.GetKey(KeyCode.Space)) {
                timestamp = Time.time;

                //These assignments clear the note display TMPs
                notesPlaying = "";
                lastPlayed = "";
                noteLogger.text = "";

                if (playback.IsRunning) {
                    playback.Stop();
                    currentNode = null; // setting this to null will end the coroutine
                } else {
                    // the linked list was generated based off of a SortedDictionary, so the first note is guaranteed the first node
                    currentNode = generator.GenerateMap(difficulty).First;

                    StartCoroutine(SpawnNotes());
                }
            }
        }
    }

    private IEnumerator SpawnNotes() {
        if (currentNode == null) yield break; // signifies either a break or the end of the song

        // start the playback in the midifile to account for any possible desync
        if (!playback.IsRunning) {
            playback.MoveToStart();
            playback.Start();
        }

        MapEvent currentEvent = currentNode.Value;
        foreach (int noteID in currentEvent.GetTilesToGenerate()) { // generates notes from the current map event
            spriteCreator.generateNote(noteID);
        }

        notesPlaying = currentEvent.GetNoteList(); // debug text for current notes (will remove later)

        long currentTimestamp = currentEvent.GetTimestamp(); // grabs current timestamp for next calculation
        currentNode = currentNode.Next;

        if (currentNode != null) { // make sure there is a next null, otherwise we do not need to wait anymore (end of song)
            float waitAmount = generator.CalculateNextTimeStamp(currentTimestamp, currentNode.Value.GetTimestamp());
            // Debug.LogFormat("Waiting {0}s for next event (TIMESTAMP {1}).", waitAmount, currentTimestamp);

            yield return new WaitForSeconds(waitAmount);
            StartCoroutine(SpawnNotes()); // recursively call subroutine for next note
        }
    }
}
