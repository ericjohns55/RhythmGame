using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;

public class GameManager : MonoBehaviour
{

    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public static bool isPaused = false;
    private static string state = "game";
    public GameObject playbackObject;
    private MidiOutput playback;
    private bool resumePlayback = false;


    Scene scene;
    public TMP_Text countdown;

    // Start is called before the first frame update
    void Start() {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        Debug.Log("Hola");
        scene = SceneManager.GetActiveScene();
        if (scene.name == "GameScene") {
            playback = (MidiOutput) playbackObject.GetComponent("MidiOutput");
        }
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if (state != "countdown") {
                if (scene.name == "GameScene") {
                    Debug.Log("Escape");
                    Debug.Log("Is paused = " + isPaused);
                    if(isPaused) {
                        ResumeGame();
                    } else  {
                        resumePlayback = playback.GetPlaybackState();
                        Debug.Log("resumePlayback = " + resumePlayback);
                        PauseGame();
                    }
                }
            }
            
            Debug.Log("Time.timeScale = " + Time.timeScale);
        }
    }

    public void PauseGame() {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        playback.StopPlayback();
    }

    public void ResumeGame() {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        StartCoroutine(Resume());        
    }

    // Separate function to resume the game
    // Called with a coroutine to allow for timer countdown
    IEnumerator Resume() {
        state = "countdown";

        for (int i = 3; i > 0; i--) {
            Debug.Log(i);
            countdown.text = i.ToString();
            yield return new WaitForSecondsRealtime(1);
        }

        countdown.text = "";

        Time.timeScale = 1f;
        isPaused = false;
        state = "game";
        if (resumePlayback) {
            playback.StartPlayback();
        }
    }

    public void PauseToSettings() {
        settingsMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }

    public void SettingsToPause() {
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void GoToMainMenu() {
        Time.timeScale = 1f;
        playback.ReleaseOutputDevice();
        SceneManager.LoadScene("MainMenu");
    }
    
    public void GoToGameScene() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        isPaused = false;
    }

    public void QuitGame() {
        Debug.Log("Quitting Game");
        Application.Quit();
    }
}