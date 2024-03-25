using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.IO;
using System.IO;
using Melanchall.DryWetMidi.Core;

public class MidiList : MonoBehaviour
{
    public string midiFolderPath = "Assets/MIDIs";
    public GameObject buttonPrefab;
    public Transform contentPanel;

    // Start is called before the first frame update
    void Start()
    {
        LoadMidiFiles();
    }

    void LoadMidiFiles()
    {
        string[] midiFiles = Directory.GetFiles(Application.dataPath + "/" + midiFolderPath, "*.mid");

        foreach (string midiFile in midiFiles) {
            string fileName = Path.GetFileNameWithoutExtension(midiFile);
            GameObject button = Instantiate(buttonPrefab, contentPanel);
            button.GetComponentInChildren<Text>().text = fileName;

            button.GetComponent<Button>().onClick.AddListener(() => LoadMidiFile(midiFile));
        }
    }

    void LoadMidiFile(string filePath)
    {
        // Load the MIDI file
        MidiFile midiFile = MidiFile.Read(filePath);

    }
}
