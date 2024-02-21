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
    public TMP_Text textElement;

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

        // prints out timing with all notes 
        IEnumerable<Note> allNotes = testMidi.GetNotes();

        foreach (Note note in allNotes) {
            Debug.Log(note.Time + " " + note.NoteName + " " + note.Octave);
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
        var notesList = e.Notes;
        foreach (Note item in notesList) {
            Debug.Log(item + " TIME: " + item.Time);
        }
    }
}
