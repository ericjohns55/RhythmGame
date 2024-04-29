using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.IO;
using TMPro;
/*
* Uses input field for the user to input their midi file path.
* Once user inputs their path, they click the "add" button and it takes 
* them to the MidiList page with their inputted path part of the 
* midi file list displayed there. 
*/

public class Settings : MonoBehaviour
{
    public GameObject inputField;

    public void Start() {
        if (inputField != null) {
            inputField.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString("MidiFilePath", "");
        }
    }

    public void SaveMidiFilePath()
    {
        string path = inputField.GetComponent<TMP_InputField>().text;

        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is empty!");
            return;
        }

        if (System.IO.Directory.Exists(path))
        {
            Debug.Log(path);
            // Saves the MIDI file path to PlayerPrefs
            PlayerPrefs.SetString("MidiFilePath", path);
        }
        else
        {
            Debug.LogError("File not found at path: " + path);
        }
    }
}