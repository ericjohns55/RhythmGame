using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class MidiList : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform contentPanel;

    private int yPosition = 0;

    // Start is called before the first frame update
    void Start()
    {
        string midiFolderPath = "MIDIs"; 
        string[] midiFiles = Directory.GetFiles(Application.dataPath + "/" + midiFolderPath, "*.mid");

        // Load MIDI files from folder
        foreach (string midiFile in midiFiles)
        {
            Debug.Log(midiFile);
            CreateMidiButton(midiFile);
            yPosition -= 5;
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
        
        button.GetComponentInChildren<TMP_Text>().text = fileName;

button.transform.SetParent(contentPanel,false);
        button.transform.SetPositionAndRotation(new Vector3(0.0f, yPosition, 0.0f), new Quaternion(0.0f, 0.0f, 0.0f,0.0f));
        

        button.GetComponent<Button>().onClick.AddListener(() => LoadMapGenerationScene(midiFilePath));
    }

    void LoadMapGenerationScene(string midiFilePath)
    {
        PlayerPrefs.SetString("SelectedMidiFilePath", midiFilePath); // Save the selected MIDI file path
        SceneManager.LoadScene("MapGenerationScene");
    }
}
