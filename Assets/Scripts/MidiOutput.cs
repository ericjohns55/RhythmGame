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
    private Dictionary<long, List<Note>> noteMap;
    private long lastTimeParsed = -1;

    private float timestamp = 0f;

    private OutputDevice outputDevice;
    private Playback playback;
 
    void Start()
    {
        MidiFile testMidi = MidiFile.Read("Assets/MIDIs/ShortSong1.mid");
        outputDevice = OutputDevice.GetByIndex(0);
        playback = testMidi.GetPlayback(outputDevice);

        playback.NotesPlaybackStarted += OnNotesPlaybackStarted;

        var allOutputs = OutputDevice.GetAll();
        foreach (var device in allOutputs) {
            Debug.Log("Output Device Found: " + device.Name);
        }

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
        if (Time.time > timestamp + 0.50f) {
            if (Input.GetKey(KeyCode.Space)) {
                timestamp = Time.time;

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

                string notesPlaying = "";

                List<Note> currentNotes;
                if (noteMap.TryGetValue(time, out currentNotes)) {
                    foreach (Note current in currentNotes) {
                        notesPlaying = notesPlaying + current.NoteName + current.Octave + " ";
                    }
                } else {
                    Debug.Log("time missing - this shouldnt happen");
                }

                notesPlaying = notesPlaying.Trim().Replace("Sharp", "#");

                Debug.Log("Time " + time + " (" + notesPlaying + ")");
            }
        } else {
            Debug.Log("how do we have a playback event without any notes???");
        }
    }
}
