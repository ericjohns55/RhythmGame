using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
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
        outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth");
        playback = testMidi.GetPlayback(outputDevice);
        playback.NotesPlaybackStarted += OnNotesPlaybackStarted;
        playback.NotesPlaybackFinished += OnNotesPlaybackFinished;
    }

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
        if (Time.time > timestamp + 0.15f) {
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
        LogNotes("Note played: ", e);
    }

    private void OnNotesPlaybackFinished(object sender, NotesEventArgs e)
    {
        LogNotes("Notes finished:", e);
    }

    private void LogNotes(string title, NotesEventArgs e)
    {
        var message = new StringBuilder()
            .AppendLine(title)
            .AppendLine(string.Join(Environment.NewLine, e.Notes.Select(n => $"  {n}")))
            .ToString();
        Debug.Log(message.Trim());
    }
}
