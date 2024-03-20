using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public GameObject midiFiles;
    public GameObject goBack;
    public GameObject settingsMenu;

    // Start is called before the first frame update
    void Start() {
        settingsMenu.SetActive(true);
    }

    public void GoToMidiFolder() {
        settingsMenu.setActive(false);
        midiFiles.setActive(true);
        SceneManager.LoadScene("MidiHandler");
    }

    public void GoBack() {
        settingsMenu.setActive(true);
        midiFiles.setActive(false);
        SceneManager.LoadScene("Pause");
    }

}