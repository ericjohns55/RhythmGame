using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using System.Text;
using System.Linq;

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
        MidiFile testMidi = MidiFile.Read("Assets/MIDIs/ShortSong1.mid");
        outputDevice = OutputDevice.GetByIndex(0);
        playback = testMidi.GetPlayback(outputDevice);

        spriteCreator = Camera.main.GetComponent<SpriteCreator>();

        playback.NotesPlaybackStarted += OnNotesPlaybackStarted;

        var allOutputs = OutputDevice.GetAll();
        foreach (var device in allOutputs) {
            Debug.Log("Output Device Found: " + device.Name);
        }

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

    void OnApplicationQuit() {
        if (playback != null) {
            playback.NotesPlaybackStarted -= OnNotesPlaybackStarted;
            playback.Dispose();
        }

        if (outputDevice != null) {
            outputDevice.Dispose();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!notesPlaying.Equals(lastPlayed)) {
            noteLogger.text = notesPlaying;
            lastPlayed = notesPlaying;
        }

        if (notesToPlayOnUpdate.Count != 0) {
            foreach (int value in notesToPlayOnUpdate) {
                spriteCreator.generateNote(value);
            }

            notesToPlayOnUpdate.Clear();
        }

        if (Time.time > timestamp + 0.50f) {
            if (Input.GetKey(KeyCode.Space)) {
                timestamp = Time.time;

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

    private void OnNotesPlaybackStarted(object sender, NotesEventArgs e)
    {
        if (e.Notes.ElementAt(0) != null) {
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

                        if (noteLookupTable.TryGetValue(current.NoteName, out noteID)) {
                            if (!notesToPlayOnUpdate.Contains(noteID)) {
                                notesToPlayOnUpdate.Add(noteID);
                            }
                        }
                    }
                } else {
                    Debug.Log("time missing - this shouldnt happen");
                }

                notesPlaying = notesPlaying.Trim().Replace("Sharp", "#");

                //Debug.Log("Time " + time + " (" + notesPlaying + ")");
            }
        } else {
            Debug.Log("how do we have a playback event without any notes???");
        }
    }
}
