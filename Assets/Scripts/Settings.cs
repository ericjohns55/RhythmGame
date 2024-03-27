using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.IO;

public class Settings : MonoBehaviour
{
    public InputField pathInputField;
    public InputField midiFilePathInput;


    public void LoadMidiFile()
    {
        string path = pathInputField.text;


        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is empty!");
            return;
        }

        if (System.IO.File.Exists(path))
        {
            string midiFilePath = midiFilePathInput.text;
            // Saves the MIDI file path to PlayerPrefs
            PlayerPrefs.SetString("MidiFilePath", midiFilePath);
            SceneManager.LoadScene("MidiListScene");
        }
        else
        {
            Debug.LogError("File not found at path: " + path);
        }
    }
}