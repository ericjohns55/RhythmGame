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
    private float contentHeight;

    
    //spacing between midi buttons
    private float spacing = 5f;

    //height of each midi button
    private float buttonHeight = 30f;

    //start is called before the first frame update
    void Start()
    {
        string midiFolderPath = PlayerPrefs.GetString("MidiFilePath", "MIDIs");
        string[] midiFiles;
        Debug.Log(midiFolderPath);
        if (midiFolderPath != "MIDIs") {
            midiFiles = Directory.GetFiles(midiFolderPath, "*.mid");
        } else { // Fallback to default unity asset path
            midiFiles = Directory.GetFiles(Application.dataPath + "/" + midiFolderPath, "*.mid");
        }

        //calculates height of content panel
        float panelHeight = midiFiles.Length * (buttonHeight + spacing);
        contentHeight = Mathf.Max(panelHeight, scrollRect.viewport.rect.height);


        //sets content panel height
        RectTransform panelRectTransform = contentPanel.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = new Vector2(panelRectTransform.sizeDelta.x, contentHeight);

        //loads MIDI files from folder
        for (int i = 0; i < midiFiles.Length; i++)
        {
            Debug.Log(midiFiles[i]);
            CreateMidiButton(midiFiles[i], i);
        }

        //retrieves MIDI file path from PlayerPrefs
        string midiFilePath = PlayerPrefs.GetString("MidiFilePath");
        if (!string.IsNullOrEmpty(midiFilePath))
        {
            //find the index of the loaded MIDI file in the midiFiles array
            int index = Array.IndexOf(midiFiles, midiFilePath);
            CreateMidiButton(midiFilePath, index);
        }
    }

    void CreateMidiButton(string midiFilePath, int index)
    {
        string fileName = Path.GetFileNameWithoutExtension(midiFilePath);
        GameObject button = Instantiate(buttonPrefab, contentPanel);
        
         //adjust the font size of the text component
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        buttonText.text = fileName;
        buttonText.fontSize = 14; 

        RectTransform buttonRectTransform = button.GetComponent<RectTransform>();
        float scrollHeight = scrollRect.viewport.rect.height;
        float scrollPosition = (contentHeight - scrollHeight) / 2;
        float yPos = -index * (buttonHeight + spacing);// - scrollPosition;

        buttonRectTransform.anchoredPosition = new Vector2(buttonRectTransform.anchoredPosition.x, yPos);

        //adjusts the width and height of the button
        buttonRectTransform.sizeDelta = new Vector2(160, buttonHeight);

        button.GetComponent<Button>().onClick.AddListener(() => StartGameWithMidi(midiFilePath));
    }

    void StartGameWithMidi(string midiFilePath) 
    {
        if (!string.IsNullOrEmpty(midiFilePath))
        {
            //saves selected Midi file path 
            PlayerPrefs.SetString("SelectedMidiFilePath", midiFilePath);
            SceneManager.LoadScene("GameScene");
        }
    }
}
