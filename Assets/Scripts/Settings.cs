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

        string midiFilePath = midiFilePathInput.text;
        // Saves the MIDI file path
        PlayerPrefs.SetString("MidiFilePath", midiFilePath);
        SceneManager.LoadScene("MidiListScene");
        
    }
}