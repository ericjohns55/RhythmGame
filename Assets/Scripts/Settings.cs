using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.IO;
/*
* Uses input field for the user to input their midi file path.
* Once user inputs their path, they click the "add" button and it takes 
* them to the MidiList page with their inputted path part of the 
* midi file list displayed there. 
*/

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