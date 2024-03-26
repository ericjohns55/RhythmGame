using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;

public class Settings : MonoBehaviour
{
    public InputField pathInputField;
    public GameObject playbackObject;
    private MidiOutput playback;

    public void LoadMidiFile()
    {
        string path = pathInputField.text;


        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is empty!");
            return;
        }

        if (File.Exists(path))
        {
            // Read MIDI file
            byte[] midiData = File.ReadAllBytes(path);
            SceneManager.LoadScene("GameScene");
            playback = (MidiOutput) playbackObject.GetComponent("MidiOutput");
        }
        else
        {
            Debug.LogError("File not found at path: " + path);
        }
    }
}