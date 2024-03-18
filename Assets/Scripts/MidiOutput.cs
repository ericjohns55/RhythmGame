using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using System.Linq;

/**
* This class currently governs midi map creation, map tracking, output device, and note creation.
* A series of Lists are implemented to keep track of notes that have been played, are currently 
* playing, and will be played as well as displaying them upon being played as a TMP.
*/
public class MidiOutput : MonoBehaviour
{
    private SpriteCreator spriteCreator;
    private Dictionary<long, List<Note>> noteMap;
    private long lastTimeParsed = -1;

    List<int> notesToPlayOnUpdate = new List<int>();

    private string notesPlaying = "";
    private string lastPlayed = "";

    private float timestamp = 0f;

    private OutputDevice outputDevice;
    private Playback playback;

    public TMP_Text noteLogger;

    private Dictionary<Melanchall.DryWetMidi.MusicTheory.NoteName, int> noteLookupTable;
 
    void Start()
    {
        MidiFile testMidi = MidiFile.Read("Assets/MIDIs/latency.mid");
        outputDevice = OutputDevice.GetByIndex(0);
        playback = testMidi.GetPlayback(outputDevice);

        spriteCreator = Camera.main.GetComponent<SpriteCreator>();

        playback.NotesPlaybackStarted += OnNotesPlaybackStarted;

        var allOutputs = OutputDevice.GetAll();
        foreach (var device in allOutputs) {
            Debug.Log("Output Device Found: " + device.Name);
        }

        //Tracks note names/types and assigns an ID value.
        noteLookupTable = new Dictionary<Melanchall.DryWetMidi.MusicTheory.NoteName, int>()
        {
            {Melanchall.DryWetMidi.MusicTheory.NoteName.C, 0},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.CSharp, 0},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.D, 1},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.DSharp, 1},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.E, 2},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.F, 3},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.FSharp, 3},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.G, 4},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.GSharp, 4},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.A, 5},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.ASharp, 5},
            {Melanchall.DryWetMidi.MusicTheory.NoteName.B, 6},
        };

        // populates the note map
        noteMap = new Dictionary<long, List<Note>>();
        IEnumerable<Note> allNotes = testMidi.GetNotes();

        foreach (Note note in allNotes) {
            long time = note.Time;

            List<Note> value;
            if (!noteMap.TryGetValue(time, out value)) {
                value = new List<Note>();
                noteMap.Add(time, value);
            }

            value.Add(note);
            //Debug.Log("Adding " + note.NoteName + note.Octave + " to time " + time);
        }
    }

    /**
    * The following function clears and releases the midi player upon closing the application.
    */
    void OnApplicationQuit() {
        ReleaseOutputDevice();
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
        * Each update any notes in the notesToPlayOnUpdate List will be generated onto the screen
        * with an assigned note prefab object.
        */
        if (notesToPlayOnUpdate.Count != 0) {
            foreach (int value in notesToPlayOnUpdate) {
                spriteCreator.generateNote(value);
            }

            notesToPlayOnUpdate.Clear();
        }

        /*
        * The following logical chain starts, stops, and resets midi playback using
        * the spacebar
        */
        if (Time.time > timestamp + 0.50f) {
            if (Input.GetKey(KeyCode.A)) {
                timestamp = Time.time;

                //These assignments clear the note display TMPs
                notesPlaying = "";
                lastPlayed = "";
                noteLogger.text = "";

                if (playback.IsRunning) {
                    playback.Stop();
                } else {
                    playback.MoveToStart();
                    playback.Start();
                }
            }
        }
    }

    /**
    * The following function governs the setup of midi playback in the system.
    * It sets the track timing, note TMP, and the List containing note order and details.
    */
    private void OnNotesPlaybackStarted(object sender, NotesEventArgs e)
    {
        if (e.Notes.ElementAt(0) != null) {
            //Gets the midi timestamp of the first note
            long time = e.Notes.ElementAt(0).Time;

            if (time != lastTimeParsed) {
                lastTimeParsed = time;

                notesPlaying = "";

                List<Note> currentNotes;
                if (noteMap.TryGetValue(time, out currentNotes)) {
                    List<Melanchall.DryWetMidi.MusicTheory.NoteName> playedNotes = new List<Melanchall.DryWetMidi.MusicTheory.NoteName>(); 

                    foreach (Note current in currentNotes) {
                        notesPlaying = notesPlaying + current.NoteName + current.Octave + " ";

                        int noteID;

                        /*
                        * Checks whether the current note exists within the lookup table and if it is not currently
                        * in the List for notes to play, it is added by ID.
                        */
                        if (noteLookupTable.TryGetValue(current.NoteName, out noteID)) {
                            if (!notesToPlayOnUpdate.Contains(noteID)) {
                                notesToPlayOnUpdate.Add(noteID);
                            }
                        }
                    }
                } else {
                    Debug.Log("time missing - this shouldnt happen");
                }

                //This removes whitespace and note accidentals from the current note string for the TMP.
                notesPlaying = notesPlaying.Trim().Replace("Sharp", "#");

                //Debug.Log("Time " + time + " (" + notesPlaying + ")");
            }
        } else {
            Debug.Log("how do we have a playback event without any notes???");
        }
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
            playback.NotesPlaybackStarted -= OnNotesPlaybackStarted;
            playback.Dispose();
        }

        if (outputDevice != null) {
            outputDevice.Dispose();
        }
    }
}
