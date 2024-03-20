using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public GameObject midiFiles;
    public GameObject goBack;
    //public GameObject settingsMenu;

    // Start is called before the first frame update
    void Start() {
        //settingsMenu.SetActive(true);
    }

    public void GoToMidiFolder() {
        goBack.SetActive(false);
        midiFiles.SetActive(true);
        SceneManager.LoadScene("MidiHandler");
    }

    public void GoBack() {
        goBack.SetActive(true);
        midiFiles.SetActive(false);
        SceneManager.LoadScene("Pause");
    }

}