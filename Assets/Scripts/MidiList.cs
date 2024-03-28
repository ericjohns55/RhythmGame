using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;
/**
* Displays all midi files in a list, with a scrolling action. 
* Each midi file is its own button.
* Once midi is selected, player uses "go" button to start the game with 
* that selected midi file.
*/

public class MidiList : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform contentPanel;
    public ScrollRect scrollRect;

    // Height of each midi button
    private float buttonHeight = 50f;
    // Spacing between midi buttons
    private float spacing = 5f;


    private int yPosition = 0;

    // Start is called before the first frame update
    void Start()
    {
        string midiFolderPath = "MIDIs"; 
        string[] midiFiles = Directory.GetFiles(Application.dataPath + "/" + midiFolderPath, "*.mid");

        // Calculates height of content panel
        float panelHeight = midiFiles.Length * (buttonHeight + spacing);

        // Sets content panel height
        RectTransform panelRectTransform = contentPanel.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = new Vector2(panelRectTransform.sizeDelta.x, panelHeight);

        // Loads MIDI files from folder
        for (int i = 0; i < midiFiles.Length; i++)
        {
            Debug.Log(midiFiles[i]);
            CreateMidiButton(midiFiles[i], i);
        }

        // Retrieves MIDI file path from PlayerPrefs
        string midiFilePath = PlayerPrefs.GetString("MidiFilePath");
        if (!string.IsNullOrEmpty(midiFilePath))
        {
            // Find the index of the loaded MIDI file in the midiFiles array
            int index = Array.IndexOf(midiFiles, midiFilePath);
            CreateMidiButton(midiFilePath, index);
        }
    }

    void CreateMidiButton(string midiFilePath, int index)
    {
        string fileName = Path.GetFileNameWithoutExtension(midiFilePath);
        GameObject button = Instantiate(buttonPrefab, contentPanel);
        
        button.GetComponentInChildren<TMP_Text>().text = fileName;

        RectTransform buttonRectTransform = button.GetComponent<RectTransform>();
        float yPos = -index * (buttonHeight + spacing);
        buttonRectTransform.anchoredPosition = new Vector2(buttonRectTransform.anchoredPosition.x, yPos);

        button.GetComponent<Button>().onClick.AddListener(() => StartGameWithMidi(midiFilePath));
    }

    void StartGameWithMidi(string midiFilePath) 
    {
        if (!string.IsNullOrEmpty(midiFilePath))
        {
            // Saves selected Midi file path 
            PlayerPrefs.SetString("SelectedMidiFilePath", midiFilePath);
            SceneManager.LoadScene("GameScene");
        }
    }
}
