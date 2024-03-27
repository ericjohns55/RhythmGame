using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MidiList : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform contentPanel;

    // Start is called before the first frame update
    void Start()
    {
        string midiFolderPath = "Assets/MIDIs"; 
        string[] midiFiles = Directory.GetFiles(Application.dataPath + "/" + midiFolderPath, "*.mid");

        // Load MIDI files from folder
        foreach (string midiFile in midiFiles)
        {
            CreateMidiButton(midiFile);
        }

        // Retrieve MIDI file path from PlayerPrefs
        string midiFilePath = PlayerPrefs.GetString("MidiFilePath");
        if (!string.IsNullOrEmpty(midiFilePath))
        {
            CreateMidiButton(midiFilePath);
        }
    }

    void CreateMidiButton(string midiFilePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(midiFilePath);
        GameObject button = Instantiate(buttonPrefab, contentPanel);
        button.GetComponentInChildren<Text>().text = fileName;

        button.GetComponent<Button>().onClick.AddListener(() => LoadMapGenerationScene(midiFilePath));
    }

    void LoadMapGenerationScene(string midiFilePath)
    {
        PlayerPrefs.SetString("SelectedMidiFilePath", midiFilePath); // Save the selected MIDI file path
        SceneManager.LoadScene("MapGenerationScene");
    }
}
